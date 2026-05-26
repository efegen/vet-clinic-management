using VetClinic.Web.ViewModels.Owners;

namespace VetClinic.Web.Services.Interfaces;

public interface IOwnerService
{
    Task<IEnumerable<OwnerListViewModel>> GetAllAsync();
    Task<OwnerDetailsViewModel?> GetDetailsAsync(int id);
    Task<OwnerCreateEditViewModel?> GetForEditAsync(int id);
    Task<Result> CreateAsync(OwnerCreateEditViewModel vm);
    Task<Result> UpdateAsync(OwnerCreateEditViewModel vm);
    Task<Result> DeleteAsync(int id);
}
