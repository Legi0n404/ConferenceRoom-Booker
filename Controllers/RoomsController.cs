using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using ConferenceRoomBooking.Models.ViewModels;

namespace ConferenceRoomBooking.Controllers;

public class RoomsController : Controller
{
    private readonly ApplicationDbContext _context;

    public RoomsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Rooms
    public async Task<IActionResult> Index(RoomListViewModel model)
    {
        IQueryable<Room> query = _context.Rooms
            .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
            .Where(r => r.IsActive);

        // Apply filters
        if (!string.IsNullOrEmpty(model.SearchTerm))
        {
            var searchTerm = model.SearchTerm.ToLower();
            query = query.Where(r => r.Name.ToLower().Contains(searchTerm) || 
                                    r.Description.ToLower().Contains(searchTerm));
        }

        if (model.MinCapacity.HasValue)
        {
            query = query.Where(r => r.Capacity >= model.MinCapacity.Value);
        }

        if (model.MaxPrice.HasValue)
        {
            query = query.Where(r => r.PricePerHour <= model.MaxPrice.Value);
        }

        // Check availability if dates provided
        if (model.AvailableFrom.HasValue && model.AvailableTo.HasValue)
        {
            var reservedRoomIds = await _context.Reservations
                .Where(res => res.Status == ReservationStatus.Approved &&
                             res.StartDate < model.AvailableTo.Value &&
                             res.EndDate > model.AvailableFrom.Value)
                .Select(res => res.RoomId)
                .Distinct()
                .ToListAsync();

            query = query.Where(r => !reservedRoomIds.Contains(r.Id));
        }

        model.Rooms = await query.OrderBy(r => r.Name).ToListAsync();
        return View(model);
    }

    // GET: Rooms/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        var viewModel = new RoomDetailsViewModel
        {
            Room = room,
            Equipment = room.RoomEquipments.Select(re => re.Equipment).ToList(),
            UpcomingReservations = await _context.Reservations
                .Where(r => r.RoomId == id && 
                           r.StartDate >= DateTime.Now &&
                           r.Status == ReservationStatus.Approved)
                .OrderBy(r => r.StartDate)
                .Take(5)
                .ToListAsync(),
            CanBook = User.Identity?.IsAuthenticated ?? false
        };

        return View(viewModel);
    }

    // GET: Rooms/Calendar/5
    public async Task<IActionResult> Calendar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        return View(room);
    }

    // GET: Rooms/GetCalendarEvents/5
    [HttpGet]
    public async Task<IActionResult> GetCalendarEvents(int id, DateTime start, DateTime end)
    {
        var reservations = await _context.Reservations
            .Where(r => r.RoomId == id &&
                       r.StartDate >= start &&
                       r.EndDate <= end &&
                       r.Status != ReservationStatus.Cancelled &&
                       r.Status != ReservationStatus.Rejected)
            .Include(r => r.User)
            .ToListAsync();

        var events = reservations.Select(r => new CalendarEventViewModel
        {
            Id = r.Id,
            Title = r.Status == ReservationStatus.Pending ? 
                   $"[Oczekuje] {r.Title}" : r.Title,
            Start = r.StartDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            End = r.EndDate.ToString("yyyy-MM-ddTHH:mm:ss"),
            Color = r.Status switch
            {
                ReservationStatus.Approved => "#28a745",
                ReservationStatus.Pending => "#ffc107",
                _ => "#6c757d"
            },
            Description = r.Description,
            RoomName = r.Room?.Name
        });

        return Json(events);
    }

    // GET: Rooms/GetAvailableSlots/5
    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(int id, DateTime date)
    {
        var reservations = await _context.Reservations
            .Where(r => r.RoomId == id && 
                       r.StartDate.Date == date.Date &&
                       (r.Status == ReservationStatus.Approved || 
                        r.Status == ReservationStatus.Pending))
            .Select(r => new { 
                start = r.StartDate.ToString("HH:mm"), 
                end = r.EndDate.ToString("HH:mm") 
            })
            .ToListAsync();

        return Json(reservations);
    }
}