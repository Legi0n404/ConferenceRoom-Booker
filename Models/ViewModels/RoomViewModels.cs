using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Models.ViewModels;

public class RoomViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa sali jest wymagana")]
    [Display(Name = "Nazwa sali")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Opis jest wymagany")]
    [Display(Name = "Opis")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Pojemność jest wymagana")]
    [Range(1, 500)]
    [Display(Name = "Pojemność")]
    public int Capacity { get; set; }

    [Required(ErrorMessage = "Cena jest wymagana")]
    [Range(0.01, 10000)]
    [Display(Name = "Cena za godzinę (PLN)")]
    public decimal PricePerHour { get; set; }

    [Display(Name = "Piętro")]
    public int Floor { get; set; }

    [Display(Name = "Numer pokoju")]
    public string? RoomNumber { get; set; }

    [Display(Name = "URL zdjęcia")]
    public string? ImageUrl { get; set; }

    [Display(Name = "Aktywna")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Wyposażenie")]
    public List<int> SelectedEquipmentIds { get; set; } = new();

    public List<Equipment>? AvailableEquipment { get; set; }
}

public class RoomDetailsViewModel
{
    public Room Room { get; set; } = null!;
    public List<Reservation> UpcomingReservations { get; set; } = new();
    public List<Equipment> Equipment { get; set; } = new();
    public bool CanBook { get; set; }
}

public class RoomListViewModel
{
    public List<Room> Rooms { get; set; } = new();
    public string? SearchTerm { get; set; }
    public int? MinCapacity { get; set; }
    public int? MaxPrice { get; set; }
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableTo { get; set; }
}