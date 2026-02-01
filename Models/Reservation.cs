using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConferenceRoomBooking.Models;

public enum ReservationStatus
{
    [Display(Name = "Oczekująca")]
    Pending = 0,
    
    [Display(Name = "Zatwierdzona")]
    Approved = 1,
    
    [Display(Name = "Odrzucona")]
    Rejected = 2,
    
    [Display(Name = "Anulowana")]
    Cancelled = 3,
    
    [Display(Name = "Zakończona")]
    Completed = 4
}

public class Reservation
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tytuł spotkania jest wymagany")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Tytuł musi mieć od 3 do 200 znaków")]
    [Display(Name = "Tytuł spotkania")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Opis może mieć maksymalnie 1000 znaków")]
    [Display(Name = "Opis / Uwagi")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Data rozpoczęcia jest wymagana")]
    [Display(Name = "Data i godzina rozpoczęcia")]
    [DataType(DataType.DateTime)]
    public DateTime StartDate { get; set; }

    [Required(ErrorMessage = "Data zakończenia jest wymagana")]
    [Display(Name = "Data i godzina zakończenia")]
    [DataType(DataType.DateTime)]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Liczba uczestników jest wymagana")]
    [Range(1, 500, ErrorMessage = "Liczba uczestników musi być między 1 a 500")]
    [Display(Name = "Liczba uczestników")]
    public int NumberOfAttendees { get; set; }

    [Display(Name = "Status")]
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    [Display(Name = "Data utworzenia")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Data modyfikacji")]
    public DateTime? ModifiedAt { get; set; }

    [Display(Name = "Komentarz admina")]
    [StringLength(500)]
    public string? AdminComment { get; set; }

    [Display(Name = "Całkowity koszt")]
    [DataType(DataType.Currency)]
    public decimal TotalCost { get; set; }

    // Foreign keys
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public int RoomId { get; set; }

    // Navigation properties
    [ForeignKey("UserId")]
    public virtual AppUser? User { get; set; }

    [ForeignKey("RoomId")]
    public virtual Room? Room { get; set; }
}