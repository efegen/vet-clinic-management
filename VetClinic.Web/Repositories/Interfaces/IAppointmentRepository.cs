using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();

    // Sunucu tarafı arama (hayvan/sahip/hizmet) + tarih aralığı + durum filtresi + sıralama + sayfalama.
    Task<(IReadOnlyList<Appointment> Items, int Total)> GetPagedAsync(
        ListQueryParams query, DateTime? from, DateTime? to, AppointmentStatus? status);
    Task<IEnumerable<Appointment>> GetByDateRangeAsync(DateTime start, DateTime end);
    Task<Appointment?> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<Appointment>> GetByPetIdAsync(int petId);
    Task<IEnumerable<Appointment>> GetConflictingAsync(DateTime requestedStart, DateTime requestedEnd, int? excludeId = null);
    Task<int> GetCountByDateAsync(DateTime date);
    Task<int> GetCountByStatusAsync(AppointmentStatus status, DateTime? fromDate = null);
}
