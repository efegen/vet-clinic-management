using VetClinic.Web.Models.Enums;
using VetClinic.Web.ViewModels.Appointments;
using VetClinic.Web.ViewModels.Common;

namespace VetClinic.Web.Services.Interfaces;

public interface IAppointmentService
{
    Task<PagedResult<AppointmentListViewModel>> GetPagedAsync(
        ListQueryParams query, DateTime? from, DateTime? to, AppointmentStatus? status);

    // Belirtilen tarihi içeren haftanın (Pazartesi–Pazar) takvim görünümü.
    Task<CalendarViewModel> GetWeekAsync(DateTime anyDateInWeek);

    Task<AppointmentDetailsViewModel?> GetDetailsAsync(int id);
    Task<AppointmentCreateEditViewModel?> GetForEditAsync(int id);
    Task<AppointmentCreateEditViewModel> BuildEmptyCreateAsync(int? petId = null);

    Task<Result> CreateAsync(AppointmentCreateEditViewModel vm);
    Task<Result> UpdateAsync(AppointmentCreateEditViewModel vm);
    Task<Result> ChangeStatusAsync(int id, AppointmentStatus newStatus);
    Task<Result> CancelAsync(int id, string? reason);
    Task<Result> DeleteAsync(int id);

    // Dropdown yeniden doldurma (POST validasyon hatasında Controller kullanır).
    Task PopulateOptionsAsync(AppointmentCreateEditViewModel vm);
}
