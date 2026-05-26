using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Repositories.Implementations;

public class OwnerRepository : Repository<Owner>, IOwnerRepository
{
    public OwnerRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<(IReadOnlyList<Owner> Items, int Total)> GetPagedAsync(ListQueryParams query)
    {
        IQueryable<Owner> q = _context.Owners;

        // Arama: ad / telefon / e-posta (LIKE — SQLite'da ASCII için büyük/küçük harf duyarsız).
        var term = query.NormalizedQ;
        if (term is not null)
        {
            var pattern = $"%{term}%";
            q = q.Where(o =>
                EF.Functions.Like(o.FullName, pattern) ||
                EF.Functions.Like(o.Phone, pattern) ||
                (o.Email != null && EF.Functions.Like(o.Email, pattern)));
        }

        var total = await q.CountAsync();

        // Sıralama (varsayılan: ada göre artan).
        q = (query.Sort?.ToLowerInvariant(), query.Descending) switch
        {
            ("phone", false) => q.OrderBy(o => o.Phone),
            ("phone", true) => q.OrderByDescending(o => o.Phone),
            ("pets", false) => q.OrderBy(o => o.Pets.Count).ThenBy(o => o.FullName),
            ("pets", true) => q.OrderByDescending(o => o.Pets.Count).ThenBy(o => o.FullName),
            ("date", false) => q.OrderBy(o => o.CreatedAt),
            ("date", true) => q.OrderByDescending(o => o.CreatedAt),
            (_, true) => q.OrderByDescending(o => o.FullName),
            _ => q.OrderBy(o => o.FullName),
        };

        var items = await q
            .Include(o => o.Pets)
            .Skip(query.Skip)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Owner?> GetByIdWithPetsAsync(int id)
        => await _context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        => await _context.Owners
            .AnyAsync(o => o.Phone == phone && (excludeId == null || o.Id != excludeId));
}
