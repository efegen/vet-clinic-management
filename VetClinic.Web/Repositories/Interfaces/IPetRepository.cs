using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetAllWithOwnerAsync();
    Task<Pet?> GetByIdWithOwnerAndAppointmentsAsync(int id);
    Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId);

    // Sunucu tarafı arama (ad/cins/sahip) + tür filtresi + sıralama + sayfalama.
    Task<(IReadOnlyList<Pet> Items, int Total)> GetPagedAsync(ListQueryParams query, PetSpecies? species);
}
