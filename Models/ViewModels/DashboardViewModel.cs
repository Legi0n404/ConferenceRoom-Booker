namespace ConferenceRoomBooking.Models.ViewModels;

public class DashboardViewModel
{
    public int TotalRooms { get; set; }
    public int ActiveRooms { get; set; }
    public int TotalReservations { get; set; }
    public int PendingReservations { get; set; }
    public int ApprovedReservations { get; set; }
    public int TodayReservations { get; set; }
    public int TotalUsers { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }

    public List<Reservation> RecentReservations { get; set; } = new();
    public List<Reservation> PendingReservationsList { get; set; } = new();
    public List<RoomStatistic> RoomStatistics { get; set; } = new();
    public List<MonthlyStatistic> MonthlyStatistics { get; set; } = new();
}

public class RoomStatistic
{
    public string RoomName { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
    public decimal Revenue { get; set; }
}

public class MonthlyStatistic
{
    public string Month { get; set; } = string.Empty;
    public int ReservationCount { get; set; }
    public decimal Revenue { get; set; }
}