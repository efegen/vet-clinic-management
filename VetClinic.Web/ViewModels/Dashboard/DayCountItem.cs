namespace VetClinic.Web.ViewModels.Dashboard;

// Hafta şeridi (Pzt..Paz) için tek bir günün randevu yoğunluğu.
// DashboardService tarafından üretilir, Dashboard'daki mini bar grafikte kullanılır.
public class DayCountItem
{
    public DayOfWeek Day { get; set; }
    public string Label { get; set; } = "";   // "Pzt", "Sal", ...
    public int Count { get; set; }
    public bool IsToday { get; set; }
    public bool IsClosed { get; set; }         // Pazar: randevu alınmaz
}
