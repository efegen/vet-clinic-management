namespace VetClinic.Web.ViewModels.Common;

// Liste sayfalarına gönderilen kabuk: sayfalı sonuç + mevcut arama/sıralama durumu (view'da geri yansıtılır).
public class ListViewModel<T>
{
    public PagedResult<T> Result { get; init; } = new();
    public string? Q { get; init; }
    public string? Sort { get; init; }
    public string Dir { get; init; } = "asc";

    // Sayfaya özel ek filtreler (ör. species, status, from, to). Sıralama linkleri ve
    // sayfalama bu değerleri korur.
    public IDictionary<string, string?> Filters { get; init; } = new Dictionary<string, string?>();

    public bool IsDescending => string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);

    // Verilen kolon için sıralama yönünü hesaplar: aktif kolona tekrar tıklanınca yön döner.
    public string NextDir(string column) =>
        string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase) && !IsDescending ? "desc" : "asc";

    public bool IsActiveSort(string column) =>
        string.Equals(Sort, column, StringComparison.OrdinalIgnoreCase);

    // Sıralama başlığı linki için route değerleri (q + filtreler korunur, sayfa 1'e döner).
    public IDictionary<string, string> SortRoute(string column)
    {
        var d = new Dictionary<string, string>();
        if (!string.IsNullOrWhiteSpace(Q)) d["q"] = Q!;
        d["sort"] = column;
        d["dir"] = NextDir(column);
        foreach (var (k, v) in Filters)
        {
            if (!string.IsNullOrWhiteSpace(v)) d[k] = v!;
        }
        return d;
    }
}
