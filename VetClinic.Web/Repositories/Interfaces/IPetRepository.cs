using VetClinic.Web.Models.Entities;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IPetRepository : IRepository<Pet>
{
    Task<IEnumerable<Pet>> GetAllWithOwnerAsync();
    Task<Pet?> GetByIdWithOwnerAndAppointmentsAsync(int id);
    Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId);
}
