using VetClinic.Web.Helpers;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Appointments;
using VetClinic.Web.ViewModels.Common;

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

    public async Task<PagedResult<AppointmentListViewModel>> GetPagedAsync(
        ListQueryParams query, DateTime? from, DateTime? to, AppointmentStatus? status)
    {
        var (items, total) = await _appointmentRepo.GetPagedAsync(query, from, to, status);

        return new PagedResult<AppointmentListViewModel>
        {
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total,
            Items = items.Select(MapToList).ToList()
        };
    }

    public async Task<CalendarViewModel> GetWeekAsync(DateTime anyDateInWeek)
    {
        var date = anyDateInWeek.Date;
        // Pazartesi'yi bul.
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        var weekStart = date.AddDays(-diff);
        var weekEnd = weekStart.AddDays(6);

        var appointments = await _appointmentRepo.GetByDateRangeAsync(weekStart, weekEnd.AddDays(1).AddTicks(-1));

        var vm = new CalendarViewModel { WeekStart = weekStart, WeekEnd = weekEnd };

        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            vm.Days.Add(new CalendarDayViewModel
            {
                Date = day,
                IsToday = day == DateTime.Today,
                IsClosed = day.DayOfWeek == DayOfWeek.Sunday,
                Items = appointments
                    .Where(a => a.AppointmentDate.Date == day)
                    .OrderBy(a => a.AppointmentDate)
                    .Select(a => new CalendarItemViewModel
                    {
                        Id = a.Id,
                        Start = a.AppointmentDate,
                        End = a.EndTime,
                        PetName = a.Pet.Name,
                        ServiceName = a.Service.Name,
                        StatusValue = a.Status,
                        Status = a.Status.ToText()
                    })
                    .ToList()
            });
        }

        return vm;
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
            StatusValue = a.Status,
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
        vm.PetChoices = pets
            .Select(p => new PetChoice(
                p.Id,
                p.Name,
                p.Owner.FullName,
                p.Species.ToText(),
                p.Species.BadgeClass()))
            .ToList();

        var services = (await _serviceRepo.GetActiveAsync()).ToList();
        // Edit'te seçili hizmet pasifleştirilmişse listede kalsın (kullanıcı kaybetmesin).
        if (vm.ServiceId != 0 && services.All(s => s.Id != vm.ServiceId))
        {
            var current = await _serviceRepo.GetByIdAsync(vm.ServiceId);
            if (current is not null) services.Insert(0, current);
        }

        vm.ServiceChoices = services
            .Select(s => new ServiceChoice(s.Id, s.Name, s.DurationMinutes, s.Price))
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
        PetId = a.PetId,
        Species = a.Pet.Species,
        OwnerName = a.Pet.Owner.FullName,
        OwnerId = a.Pet.OwnerId,
        ServiceName = a.Service.Name,
        StatusValue = a.Status,
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
