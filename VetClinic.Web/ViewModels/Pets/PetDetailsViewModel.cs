using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Pets;

public class PetDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public PetSpecies SpeciesValue { get; set; }
    public string Species { get; set; } = "";
    public string? Breed { get; set; }
    public string Gender { get; set; } = "";
    public DateTime BirthDate { get; set; }
    public string AgeText { get; set; } = "";
    public decimal? Weight { get; set; }
    public string OwnerName { get; set; } = "";
    public int OwnerId { get; set; }
    public List<AppointmentHistoryItemViewModel> Appointments { get; set; } = new();
}

public class AppointmentHistoryItemViewModel
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string ServiceName { get; set; } = "";
    public AppointmentStatus StatusValue { get; set; }
    public string Status { get; set; } = "";
}
