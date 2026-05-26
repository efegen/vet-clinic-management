namespace VetClinic.Web.ViewModels.Common;

// Liste sayfalarına gönderilen kabuk: sayfalı sonuç + mevcut arama/sıralama durumu (view'da geri yansıtılır).
public class ListViewModel<T>
{
    public PagedResult<T> Result { get; init; } = new();
    public string? Q { get; init; }
    public string? Sort { get; init; }
    public string Dir { get; init; } = "asc";

    public bool IsDescending => string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);

    // Verilen kolon için sıralama yönünü hesaplar: aktif kolona tekrar tıklanınca yön döner.
    public string NextDir(string column) =>
        string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase) && !IsDescending ? "desc" : "asc";

    public bool IsActiveSort(string column) =>
        string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase);
}
