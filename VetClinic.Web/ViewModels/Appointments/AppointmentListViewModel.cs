using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Appointments;

public class AppointmentListViewModel
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string PetName { get; set; } = "";
    public int PetId { get; set; }
    public string OwnerName { get; set; } = "";
    public int OwnerId { get; set; }
    public string ServiceName { get; set; } = "";
    public AppointmentStatus StatusValue { get; set; }
    public string Status { get; set; } = "";   // Renkli badge metni
    public int DurationMinutes { get; set; }
}
