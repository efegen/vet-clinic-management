using System.ComponentModel.DataAnnotations;

namespace VetClinic.Web.ViewModels.Owners;

public class OwnerCreateEditViewModel
{
    public int? Id { get; set; }   // null → Create, dolu → Edit

    [Required, StringLength(100, MinimumLength = 2)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = "";

    [Required, RegularExpression(@"^(\+90|0)?5\d{9}$",
        ErrorMessage = "Geçerli bir TR mobil telefon numarası giriniz.")]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = "";

    [EmailAddress, StringLength(150)]
    [Display(Name = "E-posta")]
    public string? Email { get; set; }

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }
}
