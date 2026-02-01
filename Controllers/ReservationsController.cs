using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using ConferenceRoomBooking.Models.ViewModels;

namespace ConferenceRoomBooking.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReservationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Reservations/MyReservations
    public async Task<IActionResult> MyReservations(ReservationStatus? status)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        IQueryable<Reservation> query = _context.Reservations
            .Include(r => r.Room)
            .Where(r => r.UserId == userId);

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var reservations = await query
            .OrderByDescending(r => r.StartDate)
            .ToListAsync();

        ViewBag.SelectedStatus = status;
        return View(reservations);
    }

    // GET: Reservations/Create
    public async Task<IActionResult> Create(int? roomId)
    {
        var model = new ReservationCreateViewModel
        {
            AvailableRooms = new SelectList(
                await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
                "Id", "Name"),
            Date = DateTime.Today.AddDays(1),
            StartTime = "09:00",
            EndTime = "10:00"
        };

        if (roomId.HasValue)
        {
            model.RoomId = roomId.Value;
            model.SelectedRoom = await _context.Rooms.FindAsync(roomId.Value);
        }

        return View(model);
    }

    // POST: Reservations/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReservationCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Parse times
            if (!TimeSpan.TryParse(model.StartTime, out TimeSpan startTimeSpan) ||
                !TimeSpan.TryParse(model.EndTime, out TimeSpan endTimeSpan))
            {
                ModelState.AddModelError("", "Nieprawidłowy format czasu");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            var startDate = model.Date.Date.Add(startTimeSpan);
            var endDate = model.Date.Date.Add(endTimeSpan);

            // Validation: End time must be after start time
            if (endDate <= startDate)
            {
                ModelState.AddModelError("EndTime", "Godzina zakończenia musi być późniejsza niż rozpoczęcia");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            // Validation: Date must be in the future
            if (startDate <= DateTime.Now)
            {
                ModelState.AddModelError("Date", "Data rezerwacji musi być w przyszłości");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            // Check room exists and capacity
            var room = await _context.Rooms.FindAsync(model.RoomId);
            if (room == null)
            {
                ModelState.AddModelError("RoomId", "Wybrana sala nie istnieje");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            if (model.NumberOfAttendees > room.Capacity)
            {
                ModelState.AddModelError("NumberOfAttendees", 
                    $"Liczba uczestników przekracza pojemność sali ({room.Capacity} osób)");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            // Check for conflicts
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.RoomId == model.RoomId &&
                              r.Status != ReservationStatus.Cancelled &&
                              r.Status != ReservationStatus.Rejected &&
                              r.StartDate < endDate &&
                              r.EndDate > startDate);

            if (hasConflict)
            {
                ModelState.AddModelError("", "Wybrany termin jest już zajęty. Wybierz inny termin.");
                await PrepareCreateViewModel(model);
                return View(model);
            }

            // Calculate cost
            var hours = (decimal)(endDate - startDate).TotalHours;
            var totalCost = hours * room.PricePerHour;

            var reservation = new Reservation
            {
                Title = model.Title,
                Description = model.Description,
                StartDate = startDate,
                EndDate = endDate,
                NumberOfAttendees = model.NumberOfAttendees,
                Status = ReservationStatus.Pending,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!,
                RoomId = model.RoomId,
                TotalCost = totalCost,
                CreatedAt = DateTime.Now
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została utworzona i oczekuje na zatwierdzenie.";
            return RedirectToAction(nameof(MyReservations));
        }

        await PrepareCreateViewModel(model);
        return View(model);
    }

    // GET: Reservations/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (reservation == null)
        {
            return NotFound();
        }

        // Can only edit pending reservations
        if (reservation.Status != ReservationStatus.Pending)
        {
            TempData["Error"] = "Można edytować tylko rezerwacje oczekujące na zatwierdzenie.";
            return RedirectToAction(nameof(MyReservations));
        }

        var model = new ReservationEditViewModel
        {
            Id = reservation.Id,
            RoomId = reservation.RoomId,
            Title = reservation.Title,
            Description = reservation.Description,
            Date = reservation.StartDate.Date,
            StartTime = reservation.StartDate.ToString("HH:mm"),
            EndTime = reservation.EndDate.ToString("HH:mm"),
            NumberOfAttendees = reservation.NumberOfAttendees,
            Status = reservation.Status,
            TotalCost = reservation.TotalCost,
            AvailableRooms = new SelectList(
                await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
                "Id", "Name", reservation.RoomId),
            SelectedRoom = reservation.Room
        };

        return View(model);
    }

    // POST: Reservations/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ReservationEditViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Status != ReservationStatus.Pending)
        {
            TempData["Error"] = "Można edytować tylko rezerwacje oczekujące.";
            return RedirectToAction(nameof(MyReservations));
        }

        if (ModelState.IsValid)
        {
            if (!TimeSpan.TryParse(model.StartTime, out TimeSpan startTimeSpan) ||
                !TimeSpan.TryParse(model.EndTime, out TimeSpan endTimeSpan))
            {
                ModelState.AddModelError("", "Nieprawidłowy format czasu");
                await PrepareEditViewModel(model);
                return View(model);
            }

            var startDate = model.Date.Date.Add(startTimeSpan);
            var endDate = model.Date.Date.Add(endTimeSpan);

            if (endDate <= startDate)
            {
                ModelState.AddModelError("EndTime", "Godzina zakończenia musi być późniejsza niż rozpoczęcia");
                await PrepareEditViewModel(model);
                return View(model);
            }

            // Check for conflicts (excluding current reservation)
            var hasConflict = await _context.Reservations
                .AnyAsync(r => r.RoomId == model.RoomId &&
                              r.Id != id &&
                              r.Status != ReservationStatus.Cancelled &&
                              r.Status != ReservationStatus.Rejected &&
                              r.StartDate < endDate &&
                              r.EndDate > startDate);

            if (hasConflict)
            {
                ModelState.AddModelError("", "Wybrany termin jest już zajęty.");
                await PrepareEditViewModel(model);
                return View(model);
            }

            var room = await _context.Rooms.FindAsync(model.RoomId);
            var hours = (decimal)(endDate - startDate).TotalHours;

            reservation.Title = model.Title;
            reservation.Description = model.Description;
            reservation.RoomId = model.RoomId;
            reservation.StartDate = startDate;
            reservation.EndDate = endDate;
            reservation.NumberOfAttendees = model.NumberOfAttendees;
            reservation.TotalCost = hours * room!.PricePerHour;
            reservation.ModifiedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Rezerwacja została zaktualizowana.";
            return RedirectToAction(nameof(MyReservations));
        }

        await PrepareEditViewModel(model);
        return View(model);
    }

    // POST: Reservations/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);

        if (reservation == null)
        {
            return NotFound();
        }

        if (reservation.Status == ReservationStatus.Cancelled)
        {
            TempData["Error"] = "Rezerwacja jest już anulowana.";
            return RedirectToAction(nameof(MyReservations));
        }

        if (reservation.StartDate <= DateTime.Now)
        {
            TempData["Error"] = "Nie można anulować rezerwacji, która już się rozpoczęła.";
            return RedirectToAction(nameof(MyReservations));
        }

        reservation.Status = ReservationStatus.Cancelled;
        reservation.ModifiedAt = DateTime.Now;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rezerwacja została anulowana.";
        return RedirectToAction(nameof(MyReservations));
    }

    // GET: Reservations/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isAdmin = User.IsInRole("Admin");

        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id && (r.UserId == userId || isAdmin));

        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    // AJAX: Check availability
    [HttpGet]
    public async Task<IActionResult> CheckAvailability(int roomId, DateTime date, 
        string startTime, string endTime, int? excludeId = null)
    {
        if (!TimeSpan.TryParse(startTime, out TimeSpan startTimeSpan) ||
            !TimeSpan.TryParse(endTime, out TimeSpan endTimeSpan))
        {
            return Json(new { available = false, error = "Invalid time format" });
        }

        var startDate = date.Date.Add(startTimeSpan);
        var endDate = date.Date.Add(endTimeSpan);

        IQueryable<Reservation> query = _context.Reservations
            .Where(r => r.RoomId == roomId &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected &&
                       r.StartDate < endDate &&
                       r.EndDate > startDate);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        var isAvailable = !await query.AnyAsync();
        var room = await _context.Rooms.FindAsync(roomId);
        var hours = (decimal)(endDate - startDate).TotalHours;
        var cost = hours * (room?.PricePerHour ?? 0);

        return Json(new { 
            available = isAvailable, 
            cost = cost,
            formattedCost = cost.ToString("C", new System.Globalization.CultureInfo("pl-PL"))
        });
    }

    // Helper methods
    private async Task PrepareCreateViewModel(ReservationCreateViewModel model)
    {
        model.AvailableRooms = new SelectList(
            await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
            "Id", "Name", model.RoomId);
        
        if (model.RoomId > 0)
        {
            model.SelectedRoom = await _context.Rooms.FindAsync(model.RoomId);
        }
    }

    private async Task PrepareEditViewModel(ReservationEditViewModel model)
    {
        model.AvailableRooms = new SelectList(
            await _context.Rooms.Where(r => r.IsActive).ToListAsync(),
            "Id", "Name", model.RoomId);
        
        if (model.RoomId > 0)
        {
            model.SelectedRoom = await _context.Rooms.FindAsync(model.RoomId);
        }
    }
}