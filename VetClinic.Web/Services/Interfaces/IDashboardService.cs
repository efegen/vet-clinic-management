using VetClinic.Web.ViewModels.Dashboard;

namespace VetClinic.Web.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildAsync();
}
