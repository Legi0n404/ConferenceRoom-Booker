using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Models;

public class Equipment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Nazwa wyposażenia jest wymagana")]
    [StringLength(100)]
    [Display(Name = "Nazwa")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Opis")]
    [StringLength(200)]
    public string? Description { get; set; }

    [Display(Name = "Ikona (CSS class)")]
    [StringLength(50)]
    public string? IconClass { get; set; }

    // Navigation property
    public virtual ICollection<RoomEquipment> RoomEquipments { get; set; } = new List<RoomEquipment>();
}

// Many-to-many relationship table
public class RoomEquipment
{
    public int RoomId { get; set; }
    public virtual Room Room { get; set; } = null!;

    public int EquipmentId { get; set; }
    public virtual Equipment Equipment { get; set; } = null!;

    [Display(Name = "Ilość")]
    public int Quantity { get; set; } = 1;
}