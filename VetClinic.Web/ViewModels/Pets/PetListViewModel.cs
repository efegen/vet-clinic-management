using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Pets;

public class PetListViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public PetSpecies SpeciesValue { get; set; } // Rozet rengi için
    public string Species { get; set; } = "";    // "Köpek"
    public string? Breed { get; set; }
    public string AgeText { get; set; } = "";    // "3 yıl 2 ay" / "8 aylık"
    public string OwnerName { get; set; } = "";
    public int OwnerId { get; set; }
}
