using VetClinic.Web.ViewModels.Appointments;

namespace VetClinic.Web.ViewModels.Dashboard;

public class DashboardViewModel
{
    public int TodayAppointmentCount { get; set; }
    public int ThisWeekAppointmentCount { get; set; }
    public int TotalOwnerCount { get; set; }
    public int TotalPetCount { get; set; }
    public int PendingAppointmentCount { get; set; }

    public List<AppointmentListViewModel> TodayAppointments { get; set; } = new();
    public List<AppointmentListViewModel> UpcomingAppointments { get; set; } = new();   // 7 günlük
    public List<TopServiceItem> MostRequestedServices { get; set; } = new();            // Son 30 gün
    public List<DayCountItem> WeekDistribution { get; set; } = new();                   // Pzt..Paz, 7 öğe (hafta şeridi)
}
