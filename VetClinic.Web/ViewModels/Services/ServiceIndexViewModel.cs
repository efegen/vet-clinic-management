namespace VetClinic.Web.ViewModels.Services;

public class ServiceIndexViewModel
{
    public string? Q { get; set; }
    public List<ServiceListViewModel> Active { get; set; } = new();
    public List<ServiceListViewModel> Passive { get; set; } = new();
}
