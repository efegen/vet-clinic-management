using System.ComponentModel.DataAnnotations;

namespace VetClinic.Web.Models.Entities;

public class Owner
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; set; } = "";

    [Required]
    [RegularExpression(@"^(\+90|0)?5\d{9}$")]
    public string Phone { get; set; } = "";

    [EmailAddress]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(250)]
    public string? Address { get; set; }

    // Service katmanında DateTime.Now ile set edilir (audit field — iş katmanı sorumluluğu).
    public DateTime CreatedAt { get; set; }

    // Navigation: bir sahibin birden çok hayvanı olabilir (1-N).
    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
}
