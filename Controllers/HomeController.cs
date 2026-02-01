using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;

namespace ConferenceRoomBooking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var rooms = await _context.Rooms
            .Where(r => r.IsActive)
            .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
            .OrderBy(r => r.Name)
            .Take(4)
            .ToListAsync();

        ViewBag.TotalRooms = await _context.Rooms.CountAsync(r => r.IsActive);
        ViewBag.TodayReservations = await _context.Reservations
            .CountAsync(r => r.StartDate.Date == DateTime.Today && 
                       r.Status == ReservationStatus.Approved);

        return View(rooms);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}