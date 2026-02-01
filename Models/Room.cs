using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Models;

public class Room
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa sali jest wymagana")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Nazwa musi mieć od 2 do 100 znaków")]
    [Display(Name = "Nazwa sali")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis jest wymagany")]
    [StringLength(500, ErrorMessage = "Opis może mieć maksymalnie 500 znaków")]
    [Display(Name = "Opis")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pojemność jest wymagana")]
    [Range(1, 500, ErrorMessage = "Pojemność musi być między 1 a 500")]
    [Display(Name = "Pojemność (os.)")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Cena za godzinę jest wymagana")]
    [Range(0.01, 10000, ErrorMessage = "Cena musi być między 0.01 a 10000")]
    [DataType(DataType.Currency)]
    [Display(Name = "Cena za godzinę (PLN)")]
    public decimal PricePerHour { get; set; }

    [Display(Name = "Piętro")]
    public int Floor { get; set; }

    [Display(Name = "Numer pokoju")]
    [StringLength(20)]
    public string? RoomNumber { get; set; }

    [Display(Name = "Zdjęcie URL")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Display(Name = "Aktywna")]
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}