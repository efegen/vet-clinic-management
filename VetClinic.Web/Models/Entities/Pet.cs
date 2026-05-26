using System.ComponentModel.DataAnnotations;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.Models.Entities;

public class Pet
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [Required]
    public PetSpecies Species { get; set; }

    [StringLength(50)]
    public string? Breed { get; set; }

    [Required]
    public DateTime BirthDate { get; set; }

    [Required]
    public PetGender Gender { get; set; }

    [Range(0.1, 150)]
    public decimal? Weight { get; set; }

    public int OwnerId { get; set; }

    // Navigation
    public Owner Owner { get; set; } = null!;
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
