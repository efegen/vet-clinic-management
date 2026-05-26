using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Pets;

public class PetCreateEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(50, MinimumLength = 1)]
    [Display(Name = "Hayvan Adı")]
    public string Name { get; set; } = "";

    [Required]
    [Display(Name = "Tür")]
    public PetSpecies Species { get; set; }

    [StringLength(50)]
    [Display(Name = "Cins")]
    public string? Breed { get; set; }

    [Required]
    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime BirthDate { get; set; }

    [Required]
    [Display(Name = "Cinsiyet")]
    public PetGender Gender { get; set; }

    [Range(0.1, 150)]
    [Display(Name = "Kilo (kg)")]
    public decimal? Weight { get; set; }

    [Required]
    [Display(Name = "Sahibi")]
    public int OwnerId { get; set; }

    // Dropdown için Controller/Service tarafından doldurulur.
    public List<SelectListItem> OwnerOptions { get; set; } = new();
}
