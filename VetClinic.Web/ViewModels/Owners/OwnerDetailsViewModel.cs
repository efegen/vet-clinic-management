namespace VetClinic.Web.ViewModels.Owners;

public class OwnerDetailsViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public string? Address { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PetSummaryViewModel> Pets { get; set; } = new();
}

public class PetSummaryViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Species { get; set; } = "";  // Enum → string
    public string AgeText { get; set; } = "";   // "3 yıl 2 ay"
}
