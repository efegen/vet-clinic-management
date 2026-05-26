using VetClinic.Web.Models.Entities;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IOwnerRepository : IRepository<Owner>
{
    // IRepository<Owner>'dan miras: GetAllAsync, GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync, GetTotalCountAsync

    Task<Owner?> GetByIdWithPetsAsync(int id);
    Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
}
