namespace VetClinic.Web.Helpers;

public static class AgeCalculator
{
    // Spec §8.2 algoritması. "8 aylık" / "3 yıl" / "3 yıl 2 ay" döner.
    public static string CalculateAgeText(DateTime birthDate, DateTime? today = null)
    {
        var now = today ?? DateTime.Today;

        var years = now.Year - birthDate.Year;
        var months = now.Month - birthDate.Month;
        if (now.Day < birthDate.Day) months--;
        if (months < 0) { years--; months += 12; }

        if (years == 0) return $"{months} aylık";
        if (months == 0) return $"{years} yıl";
        return $"{years} yıl {months} ay";
    }
}
