using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Helpers;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Owners;

namespace VetClinic.Web.Services.Implementations;

public class OwnerService : IOwnerService
{
    private readonly IOwnerRepository _ownerRepo;

    public OwnerService(IOwnerRepository ownerRepo)
    {
        _ownerRepo = ownerRepo;
    }

    public async Task<IEnumerable<OwnerListViewModel>> GetAllAsync()
    {
        var owners = await _ownerRepo.GetAllAsync();
        return owners
            .OrderBy(o => o.FullName)
            .Select(o => new OwnerListViewModel
            {
                Id = o.Id,
                FullName = o.FullName,
                Phone = o.Phone,
                Email = o.Email,
                PetCount = o.Pets?.Count ?? 0,
                CreatedAt = o.CreatedAt
            });
    }

    public async Task<OwnerDetailsViewModel?> GetDetailsAsync(int id)
    {
        var owner = await _ownerRepo.GetByIdWithPetsAsync(id);
        if (owner is null) return null;

        return new OwnerDetailsViewModel
        {
            Id = owner.Id,
            FullName = owner.FullName,
            Phone = owner.Phone,
            Email = owner.Email,
            Address = owner.Address,
            CreatedAt = owner.CreatedAt,
            Pets = owner.Pets
                .OrderBy(p => p.Name)
                .Select(p => new PetSummaryViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Species = p.Species.ToText(),
                    AgeText = AgeCalculator.CalculateAgeText(p.BirthDate)
                })
                .ToList()
        };
    }

    public async Task<OwnerCreateEditViewModel?> GetForEditAsync(int id)
    {
        var owner = await _ownerRepo.GetByIdAsync(id);
        if (owner is null) return null;

        return new OwnerCreateEditViewModel
        {
            Id = owner.Id,
            FullName = owner.FullName,
            Phone = owner.Phone,
            Email = owner.Email,
            Address = owner.Address
        };
    }

    public async Task<Result> CreateAsync(OwnerCreateEditViewModel vm)
    {
        if (await _ownerRepo.PhoneExistsAsync(vm.Phone))
            return Result.Fail("Bu telefon zaten kayıtlı.");

        var owner = new Owner
        {
            FullName = vm.FullName.Trim(),
            Phone = vm.Phone.Trim(),
            Email = vm.Email,
            Address = vm.Address,
            CreatedAt = DateTime.Now   // audit field — iş katmanı sorumluluğu (spec §5.1)
        };

        try
        {
            await _ownerRepo.AddAsync(owner);
        }
        catch (DbUpdateException)
        {
            // Unique index güvenlik ağı: race condition'da DB seviyesinde yakalanır (spec §5.5).
            return Result.Fail("Bu telefon zaten kayıtlı.");
        }

        return Result.Success("Sahip başarıyla eklendi.");
    }

    public async Task<Result> UpdateAsync(OwnerCreateEditViewModel vm)
    {
        if (vm.Id is null)
            return Result.Fail("Geçersiz kayıt.");

        var owner = await _ownerRepo.GetByIdAsync(vm.Id.Value);
        if (owner is null)
            return Result.Fail("Sahip bulunamadı.");

        if (await _ownerRepo.PhoneExistsAsync(vm.Phone, vm.Id))
            return Result.Fail("Bu telefon zaten kayıtlı.");

        owner.FullName = vm.FullName.Trim();
        owner.Phone = vm.Phone.Trim();
        owner.Email = vm.Email;
        owner.Address = vm.Address;

        try
        {
            await _ownerRepo.UpdateAsync(owner);
        }
        catch (DbUpdateException)
        {
            return Result.Fail("Bu telefon zaten kayıtlı.");
        }

        return Result.Success("Sahip bilgileri güncellendi.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var owner = await _ownerRepo.GetByIdAsync(id);
        if (owner is null)
            return Result.Fail("Sahip bulunamadı.");

        // Cascade: hayvanlar ve onların randevuları da silinir (DbContext yapılandırması).
        await _ownerRepo.DeleteAsync(id);
        return Result.Success("Sahip ve ilişkili kayıtlar silindi.");
    }
}
