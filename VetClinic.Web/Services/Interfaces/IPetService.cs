using VetClinic.Web.Models.Enums;
using VetClinic.Web.ViewModels.Common;
using VetClinic.Web.ViewModels.Pets;

namespace VetClinic.Web.Services.Interfaces;

public interface IPetService
{
    Task<PagedResult<PetListViewModel>> GetPagedAsync(ListQueryParams query, PetSpecies? species);
    Task<PetDetailsViewModel?> GetDetailsAsync(int id);
    Task<PetCreateEditViewModel?> GetForEditAsync(int id);
    Task<PetCreateEditViewModel> BuildEmptyCreateAsync(int? ownerId = null);
    Task<Result> CreateAsync(PetCreateEditViewModel vm);
    Task<Result> UpdateAsync(PetCreateEditViewModel vm);
    Task<Result> DeleteAsync(int id);
    string CalculateAgeText(DateTime birthDate);

    // Dropdown yeniden doldurma (POST validasyon hatasında Controller kullanır).
    Task PopulateOwnerOptionsAsync(PetCreateEditViewModel vm);
}
