using VetClinic.Web.ViewModels.Services;

namespace VetClinic.Web.Services.Interfaces;

public interface IServiceCatalogService
{
    Task<IEnumerable<ServiceListViewModel>> GetAllAsync();
    Task<ServiceCreateEditViewModel?> GetForEditAsync(int id);
    Task<Result> CreateAsync(ServiceCreateEditViewModel vm);
    Task<Result> UpdateAsync(ServiceCreateEditViewModel vm);

    Task<Result> DeactivateAsync(int id);  // IsActive = false
    Task<Result> ActivateAsync(int id);    // IsActive = true
    Task<Result> DeleteAsync(int id);      // Sadece randevusu yoksa fiziksel sil
}
