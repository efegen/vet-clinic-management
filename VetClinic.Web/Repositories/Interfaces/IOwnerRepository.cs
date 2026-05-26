using VetClinic.Web.Models.Entities;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IOwnerRepository : IRepository<Owner>
{
    // IRepository<Owner>'dan miras: GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetTotalCountAsync

    Task<Owner?> GetByIdWithPetsAsync(int id);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);

    // Sunucu tarafı arama + sıralama + sayfalama. Pets, sayfa kayıtları için yüklenir (PetCount).
    Task<(IReadOnlyList<Owner> Items, int Total)> GetPagedAsync(ListQueryParams query);
}
