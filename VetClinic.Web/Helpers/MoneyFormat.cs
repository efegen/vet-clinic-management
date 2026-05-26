using System.Globalization;

namespace VetClinic.Web.Helpers;

public static class MoneyFormat
{
    private static readonly CultureInfo Tr = new("tr-TR");

    // Decimal'i Türkçe para biçiminde gösterir: 1800 -> "1.800,00 ₺".
    // Global kültürü değiştirmeden yalnızca gösterim için kullanılır (model binding etkilenmez).
    public static string ToPriceDisplay(this decimal value) => value.ToString("N2", Tr) + " ₺";
}
