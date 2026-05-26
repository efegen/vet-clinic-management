using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinic.Web.Helpers;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Pets;

namespace VetClinic.Web.Services.Implementations;

public class PetService : IPetService
{
    private readonly IPetRepository _petRepo;
    private readonly IOwnerRepository _ownerRepo;
    private readonly ILogger<PetService> _logger;

    public PetService(IPetRepository petRepo, IOwnerRepository ownerRepo, ILogger<PetService> logger)
    {
        _petRepo = petRepo;
        _ownerRepo = ownerRepo;
        _logger = logger;
    }

    public string CalculateAgeText(DateTime birthDate) => AgeCalculator.CalculateAgeText(birthDate);

    public async Task<IEnumerable<PetListViewModel>> GetAllAsync()
    {
        var pets = await _petRepo.GetAllWithOwnerAsync();
        return pets.Select(p => new PetListViewModel
        {
            Id = p.Id,
            Name = p.Name,
            Species = p.Species.ToText(),
            Breed = p.Breed,
            AgeText = CalculateAgeText(p.BirthDate),
            OwnerName = p.Owner.FullName,
            OwnerId = p.OwnerId
        });
    }

    public async Task<PetDetailsViewModel?> GetDetailsAsync(int id)
    {
        var pet = await _petRepo.GetByIdWithOwnerAndAppointmentsAsync(id);
        if (pet is null) return null;

        return new PetDetailsViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species.ToText(),
            Breed = pet.Breed,
            Gender = pet.Gender.ToText(),
            BirthDate = pet.BirthDate,
            AgeText = CalculateAgeText(pet.BirthDate),
            Weight = pet.Weight,
            OwnerName = pet.Owner.FullName,
            OwnerId = pet.OwnerId,
            Appointments = pet.Appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentHistoryItemViewModel
                {
                    Id = a.Id,
                    AppointmentDate = a.AppointmentDate,
                    ServiceName = a.Service.Name,
                    Status = a.Status.ToText()
                })
                .ToList()
        };
    }

    public async Task<PetCreateEditViewModel?> GetForEditAsync(int id)
    {
        var pet = await _petRepo.GetByIdAsync(id);
        if (pet is null) return null;

        var vm = new PetCreateEditViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            Gender = pet.Gender,
            Weight = pet.Weight,
            OwnerId = pet.OwnerId
        };
        await PopulateOwnerOptionsAsync(vm);
        return vm;
    }

    public async Task<PetCreateEditViewModel> BuildEmptyCreateAsync(int? ownerId = null)
    {
        var vm = new PetCreateEditViewModel
        {
            BirthDate = DateTime.Today,
            OwnerId = ownerId ?? 0
        };
        await PopulateOwnerOptionsAsync(vm);
        return vm;
    }

    public async Task PopulateOwnerOptionsAsync(PetCreateEditViewModel vm)
    {
        var owners = await _ownerRepo.GetAllAsync();
        vm.OwnerOptions = owners
            .OrderBy(o => o.FullName)
            .Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = $"{o.FullName} ({o.Phone})",
                Selected = o.Id == vm.OwnerId
            })
            .ToList();
    }

    public async Task<Result> CreateAsync(PetCreateEditViewModel vm)
    {
        var validation = ValidateBirthDate(vm.BirthDate);
        if (!validation.Succeeded) return validation;

        if (await _ownerRepo.GetByIdAsync(vm.OwnerId) is null)
            return Result.Fail("Seçilen sahip bulunamadı.");

        var pet = new Pet
        {
            Name = vm.Name.Trim(),
            Species = vm.Species,
            Breed = vm.Breed,
            BirthDate = vm.BirthDate,
            Gender = vm.Gender,
            Weight = vm.Weight,
            OwnerId = vm.OwnerId
        };

        await _petRepo.AddAsync(pet);
        return Result.Success("Hayvan başarıyla eklendi.");
    }

    public async Task<Result> UpdateAsync(PetCreateEditViewModel vm)
    {
        if (vm.Id is null)
            return Result.Fail("Geçersiz kayıt.");

        var pet = await _petRepo.GetByIdAsync(vm.Id.Value);
        if (pet is null)
            return Result.Fail("Hayvan bulunamadı.");

        var validation = ValidateBirthDate(vm.BirthDate);
        if (!validation.Succeeded) return validation;

        if (await _ownerRepo.GetByIdAsync(vm.OwnerId) is null)
            return Result.Fail("Seçilen sahip bulunamadı.");

        pet.Name = vm.Name.Trim();
        pet.Species = vm.Species;
        pet.Breed = vm.Breed;
        pet.BirthDate = vm.BirthDate;
        pet.Gender = vm.Gender;
        pet.Weight = vm.Weight;
        pet.OwnerId = vm.OwnerId;

        await _petRepo.UpdateAsync(pet);
        return Result.Success("Hayvan bilgileri güncellendi.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var pet = await _petRepo.GetByIdAsync(id);
        if (pet is null)
            return Result.Fail("Hayvan bulunamadı.");

        // Cascade: hayvanın randevuları da silinir.
        await _petRepo.DeleteAsync(id);
        return Result.Success("Hayvan ve randevuları silindi.");
    }

    private Result ValidateBirthDate(DateTime birthDate)
    {
        if (birthDate.Date > DateTime.Today)
            return Result.Fail("Doğum tarihi gelecek olamaz.");

        if (birthDate.Date < DateTime.Today.AddYears(-50))
            _logger.LogWarning("Şüpheli doğum tarihi: {BirthDate} (50 yıldan eski).", birthDate);

        return Result.Success();
    }
}
