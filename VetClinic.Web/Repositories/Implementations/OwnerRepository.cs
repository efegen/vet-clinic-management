using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Repositories.Interfaces;

namespace VetClinic.Web.Repositories.Implementations;

public class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Owner?> GetByIdWithPetsAsync(int id)
        => await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        => await _context.Owners
            .AnyAsync(o => o.Phone == phone && (excludeId == null || o.Id != excludeId));
}
