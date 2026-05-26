namespace VetClinic.Web.ViewModels.Appointments;

public class AppointmentDetailsViewModel
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public DateTime EndTime { get; set; }   // Start + service duration (snapshot)
    public string Status { get; set; } = "";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PetName { get; set; } = "";
    public int PetId { get; set; }
    public string OwnerName { get; set; } = "";
    public int OwnerId { get; set; }
    public string ServiceName { get; set; } = "";
    public decimal ServicePrice { get; set; }
}
