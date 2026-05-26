namespace VetClinic.Web.ViewModels.Owners;

public class OwnerListViewModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string? Email { get; set; }
    public int PetCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
