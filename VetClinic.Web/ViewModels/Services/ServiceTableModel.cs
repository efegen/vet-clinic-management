namespace VetClinic.Web.ViewModels.Services;

// _ServiceTable partial'ı için: hizmet listesi + sekme türü (aktif/pasif buton seti).
public class ServiceTableModel
{
    public List<ServiceListViewModel> Items { get; set; } = new();
    public bool IsActiveTab { get; set; }
}
