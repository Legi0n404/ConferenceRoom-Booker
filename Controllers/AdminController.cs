using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ConferenceRoomBooking.Data;
using ConferenceRoomBooking.Models;
using ConferenceRoomBooking.Models.ViewModels;

namespace ConferenceRoomBooking.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var model = new DashboardViewModel
        {
            TotalRooms = await _context.Rooms.CountAsync(),
            ActiveRooms = await _context.Rooms.CountAsync(r => r.IsActive),
            TotalReservations = await _context.Reservations.CountAsync(),
            PendingReservations = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatus.Pending),
            ApprovedReservations = await _context.Reservations
                .CountAsync(r => r.Status == ReservationStatus.Approved),
            TodayReservations = await _context.Reservations
                .CountAsync(r => r.StartDate.Date == DateTime.Today && 
                           r.Status == ReservationStatus.Approved),
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalRevenue = await _context.Reservations
                .Where(r => r.Status == ReservationStatus.Approved || 
                           r.Status == ReservationStatus.Completed)
                .SumAsync(r => r.TotalCost),
            MonthlyRevenue = await _context.Reservations
                .Where(r => r.StartDate >= startOfMonth &&
                           (r.Status == ReservationStatus.Approved || 
                            r.Status == ReservationStatus.Completed))
                .SumAsync(r => r.TotalCost),
            RecentReservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .ToListAsync(),
            PendingReservationsList = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r => r.Status == ReservationStatus.Pending)
                .OrderBy(r => r.StartDate)
                .Take(10)
                .ToListAsync(),
            RoomStatistics = await _context.Rooms
                .Select(r => new RoomStatistic
                {
                    RoomName = r.Name,
                    ReservationCount = r.Reservations.Count(res => 
                        res.Status == ReservationStatus.Approved || 
                        res.Status == ReservationStatus.Completed),
                    Revenue = r.Reservations
                        .Where(res => res.Status == ReservationStatus.Approved || 
                                     res.Status == ReservationStatus.Completed)
                        .Sum(res => res.TotalCost)
                })
                .OrderByDescending(rs => rs.ReservationCount)
                .ToListAsync()
        };

        // Monthly statistics for chart (last 6 months)
        model.MonthlyStatistics = new List<MonthlyStatistic>();
        for (int i = 5; i >= 0; i--)
        {
            var month = now.AddMonths(-i);
            var monthStart = new DateTime(month.Year, month.Month, 1);
            var monthEnd = monthStart.AddMonths(1);

            var stat = new MonthlyStatistic
            {
                Month = month.ToString("MMM yyyy"),
                ReservationCount = await _context.Reservations
                    .CountAsync(r => r.StartDate >= monthStart && 
                                    r.StartDate < monthEnd &&
                                    (r.Status == ReservationStatus.Approved || 
                                     r.Status == ReservationStatus.Completed)),
                Revenue = await _context.Reservations
                    .Where(r => r.StartDate >= monthStart && 
                               r.StartDate < monthEnd &&
                               (r.Status == ReservationStatus.Approved || 
                                r.Status == ReservationStatus.Completed))
                    .SumAsync(r => r.TotalCost)
            };
            model.MonthlyStatistics.Add(stat);
        }

        return View(model);
    }

    // GET: Admin/ManageReservations
    public async Task<IActionResult> ManageReservations(ReservationListViewModel model)
    {
        IQueryable<Reservation> query = _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User);

        if (model.FilterStatus.HasValue)
        {
            query = query.Where(r => r.Status == model.FilterStatus.Value);
        }

        if (model.FilterDateFrom.HasValue)
        {
            query = query.Where(r => r.StartDate >= model.FilterDateFrom.Value);
        }

        if (model.FilterDateTo.HasValue)
        {
            query = query.Where(r => r.StartDate <= model.FilterDateTo.Value);
        }

        if (model.FilterRoomId.HasValue)
        {
            query = query.Where(r => r.RoomId == model.FilterRoomId.Value);
        }

        model.Reservations = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        model.Rooms = new SelectList(await _context.Rooms.ToListAsync(), "Id", "Name");

        return View(model);
    }

    // POST: Admin/UpdateReservationStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateReservationStatus(ReservationAdminViewModel model)
    {
        var reservation = await _context.Reservations.FindAsync(model.Id);
        
        if (reservation == null)
        {
            return NotFound();
        }

        reservation.Status = model.NewStatus;
        reservation.AdminComment = model.AdminComment;
        reservation.ModifiedAt = DateTime.Now;

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Status rezerwacji został zmieniony na: {model.NewStatus}";
        return RedirectToAction(nameof(ManageReservations));
    }

    // GET: Admin/ManageRooms
    public async Task<IActionResult> ManageRooms()
    {
        var rooms = await _context.Rooms
            .Include(r => r.RoomEquipments)
                .ThenInclude(re => re.Equipment)
            .OrderBy(r => r.Name)
            .ToListAsync();

        return View(rooms);
    }

    // GET: Admin/CreateRoom
    public async Task<IActionResult> CreateRoom()
    {
        var model = new RoomViewModel
        {
            AvailableEquipment = await _context.Equipment.ToListAsync()
        };
        return View(model);
    }

    // POST: Admin/CreateRoom
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoom(RoomViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Check if room name already exists
            if (await _context.Rooms.AnyAsync(r => r.Name == model.Name))
            {
                ModelState.AddModelError("Name", "Sala o tej nazwie już istnieje");
                model.AvailableEquipment = await _context.Equipment.ToListAsync();
                return View(model);
            }

            var room = new Room
            {
                Name = model.Name,
                Description = model.Description,
                Capacity = model.Capacity,
                PricePerHour = model.PricePerHour,
                Floor = model.Floor,
                RoomNumber = model.RoomNumber,
                ImageUrl = model.ImageUrl,
                IsActive = model.IsActive
            };

            _context.Rooms.Add(room);
            await _context.SaveChangesAsync();

            // Add equipment
            if (model.SelectedEquipmentIds.Any())
            {
                foreach (var equipmentId in model.SelectedEquipmentIds)
                {
                    _context.RoomEquipments.Add(new RoomEquipment
                    {
                        RoomId = room.Id,
                        EquipmentId = equipmentId,
                        Quantity = 1
                    });
                }
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Sala została utworzona.";
            return RedirectToAction(nameof(ManageRooms));
        }

        model.AvailableEquipment = await _context.Equipment.ToListAsync();
        return View(model);
    }

    // GET: Admin/EditRoom/5
    public async Task<IActionResult> EditRoom(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms
            .Include(r => r.RoomEquipments)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        var model = new RoomViewModel
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            Capacity = room.Capacity,
            PricePerHour = room.PricePerHour,
            Floor = room.Floor,
            RoomNumber = room.RoomNumber,
            ImageUrl = room.ImageUrl,
            IsActive = room.IsActive,
            SelectedEquipmentIds = room.RoomEquipments.Select(re => re.EquipmentId).ToList(),
            AvailableEquipment = await _context.Equipment.ToListAsync()
        };

        return View(model);
    }

    // POST: Admin/EditRoom/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoom(int id, RoomViewModel model)
    {
        if (id != model.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            var room = await _context.Rooms
                .Include(r => r.RoomEquipments)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            // Check if another room has the same name
            if (await _context.Rooms.AnyAsync(r => r.Name == model.Name && r.Id != id))
            {
                ModelState.AddModelError("Name", "Inna sala o tej nazwie już istnieje");
                model.AvailableEquipment = await _context.Equipment.ToListAsync();
                return View(model);
            }

            room.Name = model.Name;
            room.Description = model.Description;
            room.Capacity = model.Capacity;
            room.PricePerHour = model.PricePerHour;
            room.Floor = model.Floor;
            room.RoomNumber = model.RoomNumber;
            room.ImageUrl = model.ImageUrl;
            room.IsActive = model.IsActive;

            // Update equipment
            _context.RoomEquipments.RemoveRange(room.RoomEquipments);
            
            if (model.SelectedEquipmentIds.Any())
            {
                foreach (var equipmentId in model.SelectedEquipmentIds)
                {
                    _context.RoomEquipments.Add(new RoomEquipment
                    {
                        RoomId = room.Id,
                        EquipmentId = equipmentId,
                        Quantity = 1
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sala została zaktualizowana.";
            return RedirectToAction(nameof(ManageRooms));
        }

        model.AvailableEquipment = await _context.Equipment.ToListAsync();
        return View(model);
    }

    // POST: Admin/DeleteRoom/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await _context.Rooms.FindAsync(id);
        
        if (room == null)
        {
            return NotFound();
        }

        // Check if there are any reservations for this room
        var hasReservations = await _context.Reservations
            .AnyAsync(r => r.RoomId == id && 
                          r.Status != ReservationStatus.Cancelled &&
                          r.StartDate > DateTime.Now);

        if (hasReservations)
        {
            TempData["Error"] = "Nie można usunąć sali z aktywnymi rezerwacjami. Najpierw anuluj rezerwacje.";
            return RedirectToAction(nameof(ManageRooms));
        }

        // Soft delete - just deactivate
        room.IsActive = false;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Sala została dezaktywowana.";
        return RedirectToAction(nameof(ManageRooms));
    }

    // GET: Admin/ReservationDetails/5
    public async Task<IActionResult> ReservationDetails(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var reservation = await _context.Reservations
            .Include(r => r.Room)
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }

    // GET: Admin/Users
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();

        var userRoles = new Dictionary<string, IList<string>>();
        foreach (var user in users)
        {
            userRoles[user.Id] = await _userManager.GetRolesAsync(user);
        }

        ViewBag.UserRoles = userRoles;
        return View(users);
    }

    // POST: Admin/ToggleUserStatus/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = !user.IsActive;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = $"Status użytkownika {user.FullName} został zmieniony.";
        return RedirectToAction(nameof(Users));
    }
}