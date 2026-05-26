using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Appointments;

public class AppointmentCreateEditViewModel
{
    public int? Id { get; set; }

    [Required]
    [Display(Name = "Hayvan")]
    public int PetId { get; set; }

    [Required]
    [Display(Name = "Hizmet")]
    public int ServiceId { get; set; }

    [Required]
    [Display(Name = "Randevu Tarihi/Saati")]
    [DataType(DataType.DateTime)]
    public DateTime AppointmentDate { get; set; }

    [Display(Name = "Durum")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Beklemede;

    [StringLength(500)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }

    // Dropdown'lar
    public List<SelectListItem> PetOptions { get; set; } = new();
    public List<SelectListItem> ServiceOptions { get; set; } = new();
}
