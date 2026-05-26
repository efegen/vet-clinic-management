using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinic.Web.Helpers;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Appointments;

namespace VetClinic.Web.Services.Implementations;

public class AppointmentService : IAppointmentService
{
    private static readonly TimeSpan OpenTime = TimeSpan.FromHours(9);   // 09:00
    private static readonly TimeSpan CloseTime = TimeSpan.FromHours(19); // 19:00

    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IPetRepository _petRepo;
    private readonly IServiceRepository _serviceRepo;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepo,
        IPetRepository petRepo,
        IServiceRepository serviceRepo,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepo = appointmentRepo;
        _petRepo = petRepo;
        _serviceRepo = serviceRepo;
        _logger = logger;
    }

    public async Task<IEnumerable<AppointmentListViewModel>> GetAllAsync(
        DateTime? from = null, DateTime? to = null, AppointmentStatus? status = null)
    {
        var appointments = await _appointmentRepo.GetAllWithDetailsAsync();

        if (from.HasValue)
            appointments = appointments.Where(a => a.AppointmentDate >= from.Value.Date);
        if (to.HasValue)
            appointments = appointments.Where(a => a.AppointmentDate < to.Value.Date.AddDays(1));
        if (status.HasValue)
            appointments = appointments.Where(a => a.Status == status.Value);

        return appointments.Select(MapToList);
    }

    public async Task<AppointmentDetailsViewModel?> GetDetailsAsync(int id)
    {
        var a = await _appointmentRepo.GetByIdWithDetailsAsync(id);
        if (a is null) return null;

        return new AppointmentDetailsViewModel
        {
            Id = a.Id,
            AppointmentDate = a.AppointmentDate,
            EndTime = a.EndTime,
            Status = a.Status.ToText(),
            Notes = a.Notes,
            CreatedAt = a.CreatedAt,
            PetName = a.Pet.Name,
            PetId = a.PetId,
            OwnerName = a.Pet.Owner.FullName,
            OwnerId = a.Pet.OwnerId,
            ServiceName = a.Service.Name,
            ServicePrice = a.ServicePriceSnapshot
        };
    }

    public async Task<AppointmentCreateEditViewModel?> GetForEditAsync(int id)
    {
        var a = await _appointmentRepo.GetByIdAsync(id);
        if (a is null) return null;

        var vm = new AppointmentCreateEditViewModel
        {
            Id = a.Id,
            PetId = a.PetId,
            ServiceId = a.ServiceId,
            AppointmentDate = a.AppointmentDate,
            Status = a.Status,
            Notes = a.Notes
        };
        await PopulateOptionsAsync(vm);
        return vm;
    }

    public async Task<AppointmentCreateEditViewModel> BuildEmptyCreateAsync(int? petId = null)
    {
        var vm = new AppointmentCreateEditViewModel
        {
            PetId = petId ?? 0,
            AppointmentDate = NextDefaultSlot()
        };
        await PopulateOptionsAsync(vm);
        return vm;
    }

    public async Task PopulateOptionsAsync(AppointmentCreateEditViewModel vm)
    {
        var pets = await _petRepo.GetAllWithOwnerAsync();
        vm.PetOptions = pets
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} — {p.Owner.FullName}",
                Selected = p.Id == vm.PetId
            })
            .ToList();

        var services = (await _serviceRepo.GetActiveAsync()).ToList();
        // Edit'te seçili hizmet pasifleştirilmişse listede kalsın (kullanıcı kaybetmesin).
        if (vm.ServiceId != 0 && services.All(s => s.Id != vm.ServiceId))
        {
            var current = await _serviceRepo.GetByIdAsync(vm.ServiceId);
            if (current is not null) services.Insert(0, current);
        }

        vm.ServiceOptions = services
            .Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = $"{s.Name} ({s.DurationMinutes} dk — {s.Price:N2} TL)",
                Selected = s.Id == vm.ServiceId
            })
            .ToList();
    }

    public async Task<Result> CreateAsync(AppointmentCreateEditViewModel vm)
    {
        var pet = await _petRepo.GetByIdAsync(vm.PetId);
        if (pet is null) return Result.Fail("Seçilen hayvan bulunamadı.");

        var service = await _serviceRepo.GetByIdAsync(vm.ServiceId);
        if (service is null) return Result.Fail("Seçilen hizmet bulunamadı.");
        if (!service.IsActive) return Result.Fail("Pasif bir hizmet için randevu oluşturulamaz.");

        var end = vm.AppointmentDate.AddMinutes(service.DurationMinutes);

        var dateValidation = ValidateAppointmentDate(vm.AppointmentDate, end);
        if (!dateValidation.Succeeded) return dateValidation;

        var conflictResult = await CheckConflictAsync(vm.AppointmentDate, end, vm.PetId, excludeId: null);
        if (!conflictResult.Succeeded) return conflictResult;

        var appointment = new Appointment
        {
            PetId = vm.PetId,
            ServiceId = vm.ServiceId,
            AppointmentDate = vm.AppointmentDate,
            EndTime = end,                              // snapshot
            ServicePriceSnapshot = service.Price,        // snapshot
            Status = vm.Status,
            Notes = vm.Notes,
            CreatedAt = DateTime.Now
        };

        await _appointmentRepo.AddAsync(appointment);
        return Result.Success("Randevu başarıyla oluşturuldu.");
    }

    public async Task<Result> UpdateAsync(AppointmentCreateEditViewModel vm)
    {
        if (vm.Id is null) return Result.Fail("Geçersiz kayıt.");

        var appointment = await _appointmentRepo.GetByIdAsync(vm.Id.Value);
        if (appointment is null) return Result.Fail("Randevu bulunamadı.");

        var pet = await _petRepo.GetByIdAsync(vm.PetId);
        if (pet is null) return Result.Fail("Seçilen hayvan bulunamadı.");

        var service = await _serviceRepo.GetByIdAsync(vm.ServiceId);
        if (service is null) return Result.Fail("Seçilen hizmet bulunamadı.");

        var end = vm.AppointmentDate.AddMinutes(service.DurationMinutes);

        var dateValidation = ValidateAppointmentDate(vm.AppointmentDate, end);
        if (!dateValidation.Succeeded) return dateValidation;

        var conflictResult = await CheckConflictAsync(vm.AppointmentDate, end, vm.PetId, excludeId: vm.Id);
        if (!conflictResult.Succeeded) return conflictResult;

        appointment.PetId = vm.PetId;
        appointment.ServiceId = vm.ServiceId;
        appointment.AppointmentDate = vm.AppointmentDate;
        appointment.EndTime = end;                          // snapshot yeniden hesaplanır
        appointment.ServicePriceSnapshot = service.Price;    // snapshot yeniden hesaplanır
        appointment.Status = vm.Status;
        appointment.Notes = vm.Notes;

        await _appointmentRepo.UpdateAsync(appointment);
        return Result.Success("Randevu güncellendi.");
    }

    public async Task<Result> ChangeStatusAsync(int id, AppointmentStatus newStatus)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(id);
        if (appointment is null) return Result.Fail("Randevu bulunamadı.");

        if (!IsValidTransition(appointment.Status, newStatus))
            return Result.Fail("Bu durum geçişi geçersizdir.");

        appointment.Status = newStatus;
        await _appointmentRepo.UpdateAsync(appointment);
        return Result.Success("Randevu durumu güncellendi.");
    }

    public async Task<Result> CancelAsync(int id, string? reason)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(id);
        if (appointment is null) return Result.Fail("Randevu bulunamadı.");

        if (!IsValidTransition(appointment.Status, AppointmentStatus.IptalEdildi))
            return Result.Fail("Bu randevu iptal edilemez (zaten tamamlanmış veya iptal edilmiş).");

        // Soft cancel: fiziksel silme yok, durum + iptal sebebi.
        appointment.Status = AppointmentStatus.IptalEdildi;
        if (!string.IsNullOrWhiteSpace(reason))
            appointment.Notes = reason.Trim();

        await _appointmentRepo.UpdateAsync(appointment);
        return Result.Success("Randevu iptal edildi.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var appointment = await _appointmentRepo.GetByIdAsync(id);
        if (appointment is null) return Result.Fail("Randevu bulunamadı.");

        await _appointmentRepo.DeleteAsync(id);
        return Result.Success("Randevu silindi.");
    }

    // --- yardımcılar ---

    private static AppointmentListViewModel MapToList(Appointment a) => new()
    {
        Id = a.Id,
        AppointmentDate = a.AppointmentDate,
        PetName = a.Pet.Name,
        OwnerName = a.Pet.Owner.FullName,
        ServiceName = a.Service.Name,
        Status = a.Status.ToText(),
        DurationMinutes = (int)(a.EndTime - a.AppointmentDate).TotalMinutes
    };

    // Spec §8.4(A): tarih validasyonu.
    private static Result ValidateAppointmentDate(DateTime start, DateTime end)
    {
        if (start < DateTime.Now)
            return Result.Fail("Geçmiş bir tarihe randevu oluşturulamaz.");

        if (start.TimeOfDay < OpenTime || start.TimeOfDay > CloseTime)
            return Result.Fail("Randevu saati 09:00–19:00 arasında olmalıdır.");

        if (start.DayOfWeek == DayOfWeek.Sunday)
            return Result.Fail("Pazar günleri randevu alınamaz.");

        // Bitiş aynı gün ve 19:00'u geçmemeli.
        if (end.Date > start.Date || end.TimeOfDay > CloseTime)
            return Result.Fail("Hizmet süresi mesai saatleri içine sığmıyor.");

        return Result.Success();
    }

    // Spec §8.4(B): çakışma kontrolü.
    private async Task<Result> CheckConflictAsync(DateTime start, DateTime end, int petId, int? excludeId)
    {
        var conflicts = (await _appointmentRepo.GetConflictingAsync(start, end, excludeId)).ToList();

        if (conflicts.Count > 0)
            return Result.Fail("Bu saat aralığında zaten bir randevu bulunmaktadır.");

        // Aynı hayvanın aynı gün başka randevusu → bloklanmaz, sadece loglanır.
        var samePetSameDay = await _appointmentRepo.GetByPetIdAsync(petId);
        if (samePetSameDay.Any(a => a.Id != excludeId
                                    && a.Status != AppointmentStatus.IptalEdildi
                                    && a.AppointmentDate.Date == start.Date))
        {
            _logger.LogWarning("Aynı hayvanın ({PetId}) {Date} tarihinde birden fazla randevusu var.",
                petId, start.Date);
        }

        return Result.Success();
    }

    // Spec §8.4(C): durum geçiş makinesi.
    private static bool IsValidTransition(AppointmentStatus current, AppointmentStatus next)
        => current switch
        {
            AppointmentStatus.Beklemede => next is AppointmentStatus.Onaylandi or AppointmentStatus.IptalEdildi,
            AppointmentStatus.Onaylandi => next is AppointmentStatus.Tamamlandi or AppointmentStatus.IptalEdildi,
            _ => false   // Tamamlandi ve IptalEdildi terminaldir.
        };

    private static DateTime NextDefaultSlot()
    {
        // Yarın 09:00'a varsayılan; Pazar'a denk gelirse Pazartesi'ye kaydır.
        var slot = DateTime.Today.AddDays(1).Add(OpenTime);
        if (slot.DayOfWeek == DayOfWeek.Sunday)
            slot = slot.AddDays(1);
        return slot;
    }
}
