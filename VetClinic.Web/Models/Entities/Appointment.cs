using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.Models.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    public int PetId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public DateTime AppointmentDate { get; set; }

    // Snapshot: AppointmentDate + Service.DurationMinutes. Service katmanında hesaplanır.
    // Fiziksel sütun olarak saklanır — çakışma sorgularını SQLite uyumlu (sade tarih karşılaştırması) kılar.
    public DateTime EndTime { get; set; }

    // Snapshot: randevu oluşturulduğu/güncellendiği andaki hizmet ücreti.
    [Column(TypeName = "decimal(10,2)")]
    public decimal ServicePriceSnapshot { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Beklemede;

    [StringLength(500)]
    public string? Notes { get; set; }

    // Service katmanında DateTime.Now ile set edilir (sadece Create'te).
    public DateTime CreatedAt { get; set; }

    // Navigation
    public Pet Pet { get; set; } = null!;
    public Service Service { get; set; } = null!;
}
