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

    public static string ToText(this PetGender gender) => gender switch
    {
        PetGender.Erkek => "Erkek",
        PetGender.Disi => "Dişi",
        _ => gender.ToString()
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
