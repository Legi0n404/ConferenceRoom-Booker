using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ConferenceRoomBooking.Models.ViewModels;

public class ReservationCreateViewModel
{
    [Required(ErrorMessage = "Wybierz salę")]
    [Display(Name = "Sala")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Tytuł spotkania jest wymagany")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć od 3 do 200 znaków")]
    [Display(Name = "Tytuł spotkania")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Opis / Uwagi")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data jest wymagana")]
    [Display(Name = "Data")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Godzina rozpoczęcia jest wymagana")]
    [Display(Name = "Godzina rozpoczęcia")]
    public string StartTime { get; set; } = "09:00";

    [Required(ErrorMessage = "Godzina zakończenia jest wymagana")]
    [Display(Name = "Godzina zakończenia")]
    public string EndTime { get; set; } = "10:00";

    [Required(ErrorMessage = "Podaj liczbę uczestników")]
    [Range(1, 500, ErrorMessage = "Liczba uczestników musi być między 1 a 500")]
    [Display(Name = "Liczba uczestników")]
    public int NumberOfAttendees { get; set; } = 1;

    // For dropdowns
    public SelectList? AvailableRooms { get; set; }
    public Room? SelectedRoom { get; set; }
}

public class ReservationEditViewModel
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Wybierz salę")]
    [Display(Name = "Sala")]
    public int RoomId { get; set; }

    [Required(ErrorMessage = "Tytuł spotkania jest wymagany")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć od 3 do 200 znaków")]
    [Display(Name = "Tytuł spotkania")]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Opis / Uwagi")]
    [StringLength(1000)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data jest wymagana")]
    [Display(Name = "Data")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Godzina rozpoczęcia jest wymagana")]
    [Display(Name = "Godzina rozpoczęcia")]
    public string StartTime { get; set; } = "09:00";

    [Required(ErrorMessage = "Godzina zakończenia jest wymagana")]
    [Display(Name = "Godzina zakończenia")]
    public string EndTime { get; set; } = "10:00";

    [Required(ErrorMessage = "Podaj liczbę uczestników")]
    [Range(1, 500, ErrorMessage = "Liczba uczestników musi być między 1 a 500")]
    [Display(Name = "Liczba uczestników")]
    public int NumberOfAttendees { get; set; } = 1;
    
    public ReservationStatus Status { get; set; }
    public decimal TotalCost { get; set; }

    // For dropdowns
    public SelectList? AvailableRooms { get; set; }
    public Room? SelectedRoom { get; set; }
}

public class ReservationListViewModel
{
    public List<Reservation> Reservations { get; set; } = new();
    public ReservationStatus? FilterStatus { get; set; }
    public DateTime? FilterDateFrom { get; set; }
    public DateTime? FilterDateTo { get; set; }
    public int? FilterRoomId { get; set; }
    public SelectList? Rooms { get; set; }
}

public class ReservationAdminViewModel
{
    public int Id { get; set; }
    public ReservationStatus NewStatus { get; set; }

    [Display(Name = "Komentarz")]
    [StringLength(500)]
    public string? AdminComment { get; set; }
}