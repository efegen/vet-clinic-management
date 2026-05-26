using System.Text;

namespace VetClinic.Web.Helpers;

public static class PhoneFormat
{
    // Ham telefonu (ör. "05321112233", "+905321112233", "5321112233") okunur biçime çevirir:
    // "0 (532) 111 22 33". Tanınmayan formatta orijinali döndürür.
    public static string ToPhoneDisplay(this string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return "";

        // Sadece rakamları al.
        var sb = new StringBuilder(phone.Length);
        foreach (var ch in phone)
        {
            if (char.IsDigit(ch)) sb.Append(ch);
        }
        var digits = sb.ToString();

        // 10 haneli abone numarasına indir (başında 5 olan TR mobil).
        if (digits.Length == 12 && digits.StartsWith("90")) digits = digits[2..];
        else if (digits.Length == 11 && digits.StartsWith("0")) digits = digits[1..];

        if (digits.Length != 10 || digits[0] != '5')
            return phone; // beklenmeyen format — dokunma

        return $"0 ({digits[..3]}) {digits[3..6]} {digits[6..8]} {digits[8..10]}";
    }
}
