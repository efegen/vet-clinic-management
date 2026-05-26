namespace VetClinic.Web.ViewModels.Common;

// Liste sayfalarının ortak sorgu parametreleri (arama / sıralama / sayfalama).
public class ListQueryParams
{
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public string? Q { get; set; }
    public string? Sort { get; set; }
    public string Dir { get; set; } = "asc";   // "asc" | "desc"

    private int _page = 1;
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    private int _pageSize = DefaultPageSize;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? DefaultPageSize : (value > MaxPageSize ? MaxPageSize : value);
    }

    public bool Descending => string.Equals(Dir, "desc", StringComparison.OrdinalIgnoreCase);
    public int Skip => (Page - 1) * PageSize;

    public string? NormalizedQ => string.IsNullOrWhiteSpace(Q) ? null : Q.Trim();
}
