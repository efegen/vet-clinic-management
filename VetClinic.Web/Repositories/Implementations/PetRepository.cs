using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Repositories.Interfaces;

namespace VetClinic.Web.Repositories.Implementations;

public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Pet>> GetAllWithOwnerAsync()
        => await _context.Pets
            .Include(p => p.Owner)
            .OrderBy(p => p.Name)
            .ToListAsync();

    public async Task<Pet?> GetByIdWithOwnerAndAppointmentsAsync(int id)
        => await _context.Pets
            .Include(p => p.Owner)
            .Include(p => p.Appointments)
                .ThenInclude(a => a.Service)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Pet>> GetByOwnerIdAsync(int ownerId)
        => await _context.Pets
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .ToListAsync();
}
