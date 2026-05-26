using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Services;

namespace VetClinic.Web.Services.Implementations;

public class ServiceCatalogService : IServiceCatalogService
{
    private readonly IServiceRepository _serviceRepo;

    public ServiceCatalogService(IServiceRepository serviceRepo)
    {
        _serviceRepo = serviceRepo;
    }

    public async Task<IEnumerable<ServiceListViewModel>> GetAllAsync(string? q = null)
    {
        var services = await _serviceRepo.GetAllAsync();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            services = services.Where(s => s.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        return services.Select(s => new ServiceListViewModel
        {
            Id = s.Id,
            Name = s.Name,
            DurationMinutes = s.DurationMinutes,
            Price = s.Price,
            IsActive = s.IsActive,
            AppointmentCount = s.Appointments?.Count ?? 0,
            ApplicableSpecies = s.ApplicableSpecies.OrderBy(x => x).ToList()
        });
    }

    public async Task<ServiceCreateEditViewModel?> GetForEditAsync(int id)
    {
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service is null) return null;

        return new ServiceCreateEditViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price,
            IsActive = service.IsActive,
            ApplicableSpecies = service.ApplicableSpecies.OrderBy(x => x).ToList()
        };
    }

    public async Task<Result> CreateAsync(ServiceCreateEditViewModel vm)
    {
        var species = NormalizeSpecies(vm.ApplicableSpecies);
        if (species.Count == 0)
            return Result.Fail("En az bir hayvan türü seçmelisiniz.");

        var service = new Service
        {
            Name = vm.Name.Trim(),
            Description = vm.Description,
            DurationMinutes = vm.DurationMinutes,
            Price = vm.Price,
            IsActive = vm.IsActive,
            ApplicableSpecies = species
        };

        await _serviceRepo.AddAsync(service);
        return Result.Success("Hizmet başarıyla eklendi.");
    }

    public async Task<Result> UpdateAsync(ServiceCreateEditViewModel vm)
    {
        if (vm.Id is null)
            return Result.Fail("Geçersiz kayıt.");

        var species = NormalizeSpecies(vm.ApplicableSpecies);
        if (species.Count == 0)
            return Result.Fail("En az bir hayvan türü seçmelisiniz.");

        var service = await _serviceRepo.GetByIdAsync(vm.Id.Value);
        if (service is null)
            return Result.Fail("Hizmet bulunamadı.");

        service.Name = vm.Name.Trim();
        service.Description = vm.Description;
        service.DurationMinutes = vm.DurationMinutes;
        service.Price = vm.Price;
        service.IsActive = vm.IsActive;
        service.ApplicableSpecies = species;

        await _serviceRepo.UpdateAsync(service);
        return Result.Success("Hizmet bilgileri güncellendi.");
    }

    // Tekrarları ayıkla, sırala.
    private static List<PetSpecies> NormalizeSpecies(IEnumerable<PetSpecies> input)
        => input.Distinct().OrderBy(x => x).ToList();

    public async Task<Result> DeactivateAsync(int id)
    {
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service is null)
            return Result.Fail("Hizmet bulunamadı.");

        service.IsActive = false;
        await _serviceRepo.UpdateAsync(service);
        return Result.Success("Hizmet pasifleştirildi.");
    }

    public async Task<Result> ActivateAsync(int id)
    {
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service is null)
            return Result.Fail("Hizmet bulunamadı.");

        service.IsActive = true;
        await _serviceRepo.UpdateAsync(service);
        return Result.Success("Hizmet aktifleştirildi.");
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var service = await _serviceRepo.GetByIdAsync(id);
        if (service is null)
            return Result.Fail("Hizmet bulunamadı.");

        if (await _serviceRepo.HasAppointmentsAsync(id))
            return Result.Fail("Bu hizmetin randevuları olduğundan silinemez. Bunun yerine pasifleştirin.");

        await _serviceRepo.DeleteAsync(id);
        return Result.Success("Hizmet silindi.");
    }
}
