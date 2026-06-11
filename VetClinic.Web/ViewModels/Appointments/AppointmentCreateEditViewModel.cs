using System.ComponentModel.DataAnnotations;
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

    // Dropdown verileri. Seçeneklere gömülen metadata (data-*) sağdaki canlı
    // randevu özetini (bitiş saati, süre, tahmini ücret, tür rozeti) JS ile besler.
    public List<PetChoice> PetChoices { get; set; } = new();
    public List<ServiceChoice> ServiceChoices { get; set; } = new();
}

// Hayvan seçeneği + canlı önizleme için sahip adı ve tür rozeti bilgisi.
public record PetChoice(int Id, string Name, string OwnerName, string Species, string SpeciesBadge);

// Hizmet seçeneği + bitiş saati ve tahmini ücret hesaplaması için süre/ücret.
public record ServiceChoice(int Id, string Name, int DurationMinutes, decimal Price);
