namespace VetClinic.Web.ViewModels.Common;

// Sayfalanmış sorgu sonucu — tüm liste sayfalarında kullanılır.
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int TotalCount { get; init; }

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
    public int FirstItemIndex => TotalCount == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int LastItemIndex => Math.Min(Page * PageSize, TotalCount);
}
