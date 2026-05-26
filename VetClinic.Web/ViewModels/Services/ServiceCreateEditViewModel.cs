using System.ComponentModel.DataAnnotations;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Services;

public class ServiceCreateEditViewModel
{
    public int? Id { get; set; }

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Hizmet Adı")]
    public string Name { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required, Range(15, 240)]
    [Display(Name = "Süre (dk)")]
    public int DurationMinutes { get; set; }

    [Required, Range(0.01, 100000)]
    [Display(Name = "Ücret (TL)")]
    public decimal Price { get; set; }

    [Display(Name = "Aktif mi?")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Uygulanabilir Türler")]
    public List<PetSpecies> ApplicableSpecies { get; set; } = new();
}
