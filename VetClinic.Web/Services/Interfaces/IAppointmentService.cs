using VetClinic.Web.Models.Enums;
using VetClinic.Web.ViewModels.Appointments;

namespace VetClinic.Web.Services.Interfaces;

public interface IAppointmentService
{
    Task<IEnumerable<AppointmentListViewModel>> GetAllAsync(
        DateTime? from = null, DateTime? to = null, AppointmentStatus? status = null);

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
