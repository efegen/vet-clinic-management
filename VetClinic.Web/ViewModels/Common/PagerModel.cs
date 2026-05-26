namespace VetClinic.Web.ViewModels.Common;

// _Pager partial'ına gönderilen model. RouteValues, mevcut arama/sıralama gibi
// parametreleri sayfa linklerinde korumak için kullanılır.
public class PagerModel
{
    public int Page { get; init; }
    public int TotalPages { get; init; }
    public string Action { get; init; } = "Index";

    // Linklerde korunacak ek parametreler (q, sort, dir...). page bunun üzerine eklenir.
    public IDictionary<string, string?> RouteValues { get; init; } = new Dictionary<string, string?>();

    public IDictionary<string, string> RouteFor(int page)
    {
        var d = new Dictionary<string, string>();
        foreach (var (k, v) in RouteValues)
        {
            if (!string.IsNullOrWhiteSpace(v))
                d[k] = v!;
        }
        d["page"] = page.ToString();
        return d;
    }

    public static PagerModel From<T>(PagedResult<T> result, string? q, string? sort, string? dir, string action = "Index")
        => new()
        {
            Page = result.Page,
            TotalPages = result.TotalPages,
            Action = action,
            RouteValues = new Dictionary<string, string?>
            {
                ["q"] = q,
                ["sort"] = sort,
                ["dir"] = dir
            }
        };
}
