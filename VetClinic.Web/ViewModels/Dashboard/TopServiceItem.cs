namespace VetClinic.Web.ViewModels.Dashboard;

// Son 30 günde en çok talep edilen hizmet bilgisi.
// Repository tarafından üretilir, Dashboard view'ında kullanılır.
public class TopServiceItem
{
    public string ServiceName { get; set; } = "";
    public int Count { get; set; }
}
