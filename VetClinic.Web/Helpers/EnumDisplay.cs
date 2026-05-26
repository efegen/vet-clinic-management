using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.Helpers;

// Enum değerlerinin Türkçe (diakritikli) görünüm metinleri. Enum üyeleri ASCII tutulur (Kopek, Kus...),
// kullanıcıya gösterilen metin buradan üretilir.
public static class EnumDisplay
{
    public static string ToText(this PetSpecies species) => species switch
    {
        PetSpecies.Kopek => "Köpek",
        PetSpecies.Kedi => "Kedi",
        PetSpecies.Kus => "Kuş",
        PetSpecies.Tavsan => "Tavşan",
        PetSpecies.Diger => "Diğer",
        _ => species.ToString()
    };

    // Tür rozeti rengi (spec §10.4): Köpek=primary, Kedi=warning, Kuş=info, Tavşan=success, Diğer=secondary.
    public static string BadgeClass(this PetSpecies species) => species switch
    {
        PetSpecies.Kopek => "text-bg-primary",
        PetSpecies.Kedi => "text-bg-warning",
        PetSpecies.Kus => "text-bg-info",
        PetSpecies.Tavsan => "text-bg-success",
        _ => "text-bg-secondary"
    };

    public static string ToText(this PetGender gender) => gender switch
    {
        PetGender.Erkek => "Erkek",
        PetGender.Disi => "Dişi",
        _ => gender.ToString()
    };

    // Randevu durumu rozeti rengi (spec §10.6).
    public static string BadgeClass(this AppointmentStatus status) => status switch
    {
        AppointmentStatus.Beklemede => "text-bg-warning",
        AppointmentStatus.Onaylandi => "text-bg-info",
        AppointmentStatus.Tamamlandi => "text-bg-success",
        AppointmentStatus.IptalEdildi => "text-bg-secondary",
        _ => "text-bg-light"
    };

    public static string ToText(this AppointmentStatus status) => status switch
    {
        AppointmentStatus.Beklemede => "Beklemede",
        AppointmentStatus.Onaylandi => "Onaylandı",
        AppointmentStatus.Tamamlandi => "Tamamlandı",
        AppointmentStatus.IptalEdildi => "İptal Edildi",
        _ => status.ToString()
    };
}
