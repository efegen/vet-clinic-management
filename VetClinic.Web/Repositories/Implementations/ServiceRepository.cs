using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.ViewModels.Dashboard;

namespace VetClinic.Web.Repositories.Implementations;

public class ServiceRepository : Repository<Service>, IServiceRepository
{
    public ServiceRepository(ApplicationDbContext context) : base(context)
    {
    }

    // Liste görünümü randevu sayısına ihtiyaç duyduğu için Appointments include edilir.
    public override async Task<IEnumerable<Service>> GetAllAsync()
        => await _context.Services
            .Include(s => s.Appointments)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<IEnumerable<Service>> GetActiveAsync()
        => await _context.Services
            .Where(s => s.IsActive)
            .OrderBy(s => s.Name)
            .ToListAsync();

    public async Task<bool> HasAppointmentsAsync(int serviceId)
        => await _context.Appointments.AnyAsync(a => a.ServiceId == serviceId);

    // Son `days` gün içindeki (iptal hariç) randevulara göre en çok talep gören `top` hizmet.
    public async Task<List<TopServiceItem>> GetTopRequestedAsync(int days, int top)
    {
        var since = DateTime.Now.AddDays(-days);

        return await _context.Appointments
            .Where(a => a.Status != AppointmentStatus.IptalEdildi)
            .Where(a => a.AppointmentDate >= since)
            .GroupBy(a => a.Service.Name)
            .Select(g => new TopServiceItem { ServiceName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .ToListAsync();
    }
}
