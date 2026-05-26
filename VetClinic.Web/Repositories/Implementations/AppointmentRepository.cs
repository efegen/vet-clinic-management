using Microsoft.EntityFrameworkCore;
using VetClinic.Web.Data;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;

namespace VetClinic.Web.Repositories.Implementations;

public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Appointment>> GetAllWithDetailsAsync()
        => await _context.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Service)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

    public async Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end)
        => await _context.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Service)
            .Where(a => a.AppointmentDate >= start && a.AppointmentDate <= end)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

    public async Task<Appointment?> GetByIdWithDetailsAsync(int id)
        => await _context.Appointments
            .Include(a => a.Pet).ThenInclude(p => p.Owner)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<IEnumerable<Appointment>> GetByPetIdAsync(int petId)
        => await _context.Appointments
            .Include(a => a.Service)
            .Where(a => a.PetId == petId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

    // Çakışma formülü: A.start < B.end AND A.end > B.start.
    // EndTime fiziksel sütun olduğu için sorgu SQLite'a sade tarih karşılaştırması olarak çevrilir.
    public async Task<IEnumerable<Appointment>> GetConflictingAsync(
        DateTime requestedStart, DateTime requestedEnd, int? excludeId = null)
    {
        return await _context.Appointments
            .Include(a => a.Service)
            .Include(a => a.Pet)
            .Where(a => a.Status != AppointmentStatus.IptalEdildi)
            .Where(a => excludeId == null || a.Id != excludeId)
            .Where(a => a.AppointmentDate < requestedEnd && a.EndTime > requestedStart)
            .ToListAsync();
    }

    public async Task<int> GetCountByDateAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);
        return await _context.Appointments
            .CountAsync(a => a.AppointmentDate >= dayStart && a.AppointmentDate < dayEnd);
    }

    public async Task<int> GetCountByStatusAsync(AppointmentStatus status)
        => await _context.Appointments.CountAsync(a => a.Status == status);
}
