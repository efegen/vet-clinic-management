using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Repositories.Implementations;

public class PetRepository : Repository<Pet>, IPetRepository
{
    public PetRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<Pet> Items, int Total)> GetPagedAsync(ListQueryParams query, PetSpecies? species)
    {
        IQueryable<Pet> q = _context.Pets.Include(p => p.Owner);

        var term = query.NormalizedQ;
        if (term is not null)
        {
            var pattern = $"%{term}%";
            q = q.Where(p =>
                EF.Functions.Like(p.Name, pattern) ||
                (p.Breed != null && EF.Functions.Like(p.Breed, pattern)) ||
                EF.Functions.Like(p.Owner.FullName, pattern));
        }

        if (species.HasValue)
            q = q.Where(p => p.Species == species.Value);

        var total = await q.CountAsync();

        q = (query.Sort?.ToLowerInvariant(), query.Descending) switch
        {
            ("species", false) => q.OrderBy(p => p.Species).ThenBy(p => p.Name),
            ("species", true) => q.OrderByDescending(p => p.Species).ThenBy(p => p.Name),
            // Yaş = doğum tarihi. Artan yön = en yaşlı (en eski doğum) önce.
            ("age", false) => q.OrderBy(p => p.BirthDate),
            ("age", true) => q.OrderByDescending(p => p.BirthDate),
            ("owner", false) => q.OrderBy(p => p.Owner.FullName).ThenBy(p => p.Name),
            ("owner", true) => q.OrderByDescending(p => p.Owner.FullName).ThenBy(p => p.Name),
            (_, true) => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Name),
        };

        var items = await q.Skip(query.Skip).Take(query.PageSize).ToListAsync();
        return (items, total);
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
