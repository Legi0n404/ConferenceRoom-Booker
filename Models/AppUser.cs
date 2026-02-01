using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ConferenceRoomBooking.Models;

public class AppUser : IdentityUser
{
    [Required(ErrorMessage = "Imię jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Imię musi mieć od 2 do 50 znaków")]
    [Display(Name = "Imię")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nazwisko jest wymagane")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nazwisko musi mieć od 2 do 50 znaków")]
    [Display(Name = "Nazwisko")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Firma / Dział")]
    [StringLength(100)]
    public string? Company { get; set; }

    [Display(Name = "Data rejestracji")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Display(Name = "Aktywny")]
    public bool IsActive { get; set; } = true;

    // Computed property
    public string FullName => $"{FirstName} {LastName}";

    // Navigation property
    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}