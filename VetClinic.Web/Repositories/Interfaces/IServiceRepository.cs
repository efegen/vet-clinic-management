using VetClinic.Web.Models.Entities;
using VetClinic.Web.ViewModels.Dashboard;

namespace VetClinic.Web.Repositories.Interfaces;

public interface IServiceRepository : IRepository<Service>
{
    Task<IEnumerable<Service>> GetActiveAsync();
    Task<bool> HasAppointmentsAsync(int serviceId);
    Task<List<TopServiceItem>> GetTopRequestedAsync(int days, int top);
}
