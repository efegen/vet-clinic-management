using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetClinic.Web.Models.Entities;

public class Service
{
    public int Id { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = "";

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(15, 240)]
    public int DurationMinutes { get; set; }

    [Range(0.01, 100000)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    // Pasifleştirilen hizmet yeni randevu listesinde gözükmez (soft delete).
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}
