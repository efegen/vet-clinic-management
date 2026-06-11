using VetClinic.Web.Helpers;
using VetClinic.Web.Models.Entities;
using VetClinic.Web.Models.Enums;
using VetClinic.Web.Repositories.Interfaces;
using VetClinic.Web.Services.Interfaces;
using VetClinic.Web.ViewModels.Appointments;
using VetClinic.Web.ViewModels.Dashboard;

namespace VetClinic.Web.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IAppointmentRepository _appointmentRepo;
    private readonly IOwnerRepository _ownerRepo;
    private readonly IPetRepository _petRepo;
    private readonly IServiceRepository _serviceRepo;

    public DashboardService(
        IAppointmentRepository appointmentRepo,
        IOwnerRepository ownerRepo,
        IPetRepository petRepo,
        IServiceRepository serviceRepo)
    {
        _appointmentRepo = appointmentRepo;
        _ownerRepo = ownerRepo;
        _petRepo = petRepo;
        _serviceRepo = serviceRepo;
    }

    public async Task<DashboardViewModel> BuildAsync()
    {
        var today = DateTime.Today;

        // Bu hafta: Pazartesi–Pazar.
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var weekStart = today.AddDays(-diff);
        var weekEnd = weekStart.AddDays(6);

        var todayList = (await _appointmentRepo.GetByDateRangeAsync(today, EndOfDay(today))).ToList();
        var weekList = (await _appointmentRepo.GetByDateRangeAsync(weekStart, EndOfDay(weekEnd))).ToList();
        var upcomingList = (await _appointmentRepo.GetByDateRangeAsync(today.AddDays(1), EndOfDay(today.AddDays(7)))).ToList();

        // Hafta şeridi: Pzt..Paz, her gün için randevu sayısı (mini bar grafik).
        var dayLabels = new[] { "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt", "Paz" };
        var weekDistribution = new List<DayCountItem>(7);
        for (int i = 0; i < 7; i++)
        {
            var day = weekStart.AddDays(i);
            weekDistribution.Add(new DayCountItem
            {
                Day = day.DayOfWeek,
                Label = dayLabels[i],
                Count = weekList.Count(a => a.AppointmentDate.Date == day),
                IsToday = day == today,
                IsClosed = day.DayOfWeek == DayOfWeek.Sunday   // Spec §8.4: Pazar kapalı.
            });
        }

        return new DashboardViewModel
        {
            TodayAppointmentCount = await _appointmentRepo.GetCountByDateAsync(today),
            ThisWeekAppointmentCount = weekList.Count,
            TotalOwnerCount = await _ownerRepo.GetTotalCountAsync(),
            TotalPetCount = await _petRepo.GetTotalCountAsync(),
            PendingAppointmentCount = await _appointmentRepo.GetCountByStatusAsync(AppointmentStatus.Beklemede, today),

            TodayAppointments = todayList
                .OrderBy(a => a.AppointmentDate)
                .Take(20)
                .Select(MapToList)
                .ToList(),

            UpcomingAppointments = upcomingList
                .OrderBy(a => a.AppointmentDate)
                .Take(10)
                .Select(MapToList)
                .ToList(),

            MostRequestedServices = await _serviceRepo.GetTopRequestedAsync(days: 30, top: 5),

            WeekDistribution = weekDistribution
        };
    }

    private static DateTime EndOfDay(DateTime day) => day.Date.AddDays(1).AddTicks(-1);

    private static AppointmentListViewModel MapToList(Appointment a) => new()
    {
        Id = a.Id,
        AppointmentDate = a.AppointmentDate,
        PetName = a.Pet.Name,
        PetId = a.PetId,
        Species = a.Pet.Species,
        OwnerName = a.Pet.Owner.FullName,
        OwnerId = a.Pet.OwnerId,
        ServiceName = a.Service.Name,
        StatusValue = a.Status,
        Status = a.Status.ToText(),
        DurationMinutes = (int)(a.EndTime - a.AppointmentDate).TotalMinutes
    };
}
