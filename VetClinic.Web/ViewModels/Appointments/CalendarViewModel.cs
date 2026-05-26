using VetClinic.Web.Models.Enums;

namespace VetClinic.Web.ViewModels.Appointments;

public class CalendarViewModel
{
    public DateTime WeekStart { get; set; }              // Pazartesi
    public DateTime WeekEnd { get; set; }                // Pazar
    public List<CalendarDayViewModel> Days { get; set; } = new();

    // Mesai penceresi (takvim ızgarasının sınırları).
    public int StartHour { get; set; } = 9;
    public int EndHour { get; set; } = 19;
    public int TotalMinutes => (EndHour - StartHour) * 60;

    public DateTime PreviousWeek => WeekStart.AddDays(-7);
    public DateTime NextWeek => WeekStart.AddDays(7);
    public bool IsCurrentWeek => DateTime.Today >= WeekStart && DateTime.Today <= WeekEnd;
}

public class CalendarDayViewModel
{
    public DateTime Date { get; set; }
    public bool IsToday { get; set; }
    public bool IsClosed { get; set; }   // Pazar: randevu alınmaz
    public List<CalendarItemViewModel> Items { get; set; } = new();
}

public class CalendarItemViewModel
{
    public int Id { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string PetName { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public AppointmentStatus StatusValue { get; set; }
    public string Status { get; set; } = "";

    public int DurationMinutes => (int)(End - Start).TotalMinutes;
}
