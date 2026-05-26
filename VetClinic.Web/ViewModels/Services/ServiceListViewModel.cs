namespace VetClinic.Web.ViewModels.Services;

public class ServiceListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; }
    public int AppointmentCount { get; set; }
}
