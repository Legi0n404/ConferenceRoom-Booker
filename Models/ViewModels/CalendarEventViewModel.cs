namespace ConferenceRoomBooking.Models.ViewModels;

public class CalendarEventViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Start { get; set; } = string.Empty;
    public string End { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Url { get; set; }
    public bool AllDay { get; set; } = false;
    public string? Description { get; set; }
    public string? RoomName { get; set; }
}