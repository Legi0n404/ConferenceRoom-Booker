using Microsoft.AspNetCore.Identity;
using ConferenceRoomBooking.Models;

namespace ConferenceRoomBooking.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Seed roles
        string[] roles = { "Admin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed admin user
        var adminEmail = "admin@conferenceroom.pl";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "Systemowy",
                Company = "IT Department",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }

        // Seed test user
        var userEmail = "user@conferenceroom.pl";
        var testUser = await userManager.FindByEmailAsync(userEmail);
        if (testUser == null)
        {
            testUser = new AppUser
            {
                UserName = userEmail,
                Email = userEmail,
                FirstName = "Jan",
                LastName = "Kowalski",
                Company = "Marketing",
                EmailConfirmed = true,
                IsActive = true
            };
            var result = await userManager.CreateAsync(testUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(testUser, "User");
            }
        }

        // Seed equipment
        if (!context.Equipment.Any())
        {
            var equipment = new List<Equipment>
            {
                new() { Name = "Projektor", Description = "Projektor multimedialny", IconClass = "bi-projector" },
                new() { Name = "Tablica", Description = "Tablica suchościeralna", IconClass = "bi-easel" },
                new() { Name = "Wideokonferencja", Description = "System wideokonferencyjny", IconClass = "bi-camera-video" },
                new() { Name = "Nagłośnienie", Description = "System nagłośnienia", IconClass = "bi-speaker" },
                new() { Name = "Klimatyzacja", Description = "Klimatyzacja", IconClass = "bi-snow" },
                new() { Name = "TV", Description = "Telewizor / Monitor", IconClass = "bi-tv" },
                new() { Name = "WiFi", Description = "Szybkie WiFi", IconClass = "bi-wifi" },
                new() { Name = "Flipchart", Description = "Flipchart z markerami", IconClass = "bi-clipboard" }
            };
            context.Equipment.AddRange(equipment);
            await context.SaveChangesAsync();
        }

        // Seed rooms
        if (!context.Rooms.Any())
        {
            var rooms = new List<Room>
            {
                new()
                {
                    Name = "Sala Konferencyjna A",
                    Description = "Duża sala konferencyjna z pełnym wyposażeniem multimedialnym. Idealna na prezentacje i szkolenia.",
                    Capacity = 30,
                    PricePerHour = 150.00m,
                    Floor = 1,
                    RoomNumber = "101",
                    ImageUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=800",
                    IsActive = true
                },
                new()
                {
                    Name = "Sala Konferencyjna B",
                    Description = "Średnia sala konferencyjna, idealna na spotkania zespołowe i warsztaty.",
                    Capacity = 15,
                    PricePerHour = 100.00m,
                    Floor = 1,
                    RoomNumber = "102",
                    ImageUrl = "https://images.unsplash.com/photo-1497366811353-6870744d04b2?w=800",
                    IsActive = true
                },
                new()
                {
                    Name = "Sala Spotkań C",
                    Description = "Mniejsza sala do kameralnych spotkań i rozmów rekrutacyjnych.",
                    Capacity = 8,
                    PricePerHour = 60.00m,
                    Floor = 2,
                    RoomNumber = "201",
                    ImageUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=800",
                    IsActive = true
                },
                new()
                {
                    Name = "Sala Zarządu",
                    Description = "Ekskluzywna sala z pełnym wyposażeniem do spotkań zarządu i ważnych negocjacji.",
                    Capacity = 12,
                    PricePerHour = 200.00m,
                    Floor = 3,
                    RoomNumber = "301",
                    ImageUrl = "https://images.unsplash.com/photo-1431540015161-0bf868a2d407?w=800",
                    IsActive = true
                },
                new()
                {
                    Name = "Open Space Meeting",
                    Description = "Otwarta przestrzeń do kreatywnych spotkań i burzy mózgów.",
                    Capacity = 20,
                    PricePerHour = 80.00m,
                    Floor = 2,
                    RoomNumber = "202",
                    ImageUrl = "https://images.unsplash.com/photo-1527192491265-7e15c55b1ed2?w=800",
                    IsActive = true
                }
            };

            context.Rooms.AddRange(rooms);
            await context.SaveChangesAsync();

            // Add equipment to rooms
            var allEquipment = context.Equipment.ToList();
            var allRooms = context.Rooms.ToList();

            // Room A - full equipment
            var roomA = allRooms.First(r => r.Name.Contains("A"));
            foreach (var eq in allEquipment)
            {
                context.RoomEquipments.Add(new RoomEquipment
                {
                    RoomId = roomA.Id,
                    EquipmentId = eq.Id,
                    Quantity = 1
                });
            }

            // Room B - selected equipment
            var roomB = allRooms.First(r => r.Name.Contains("B"));
            var equipmentForB = allEquipment.Where(e => 
                e.Name == "Projektor" || e.Name == "Tablica" || 
                e.Name == "WiFi" || e.Name == "Klimatyzacja").ToList();
            foreach (var eq in equipmentForB)
            {
                context.RoomEquipments.Add(new RoomEquipment
                {
                    RoomId = roomB.Id,
                    EquipmentId = eq.Id,
                    Quantity = 1
                });
            }

            // Room C
            var roomC = allRooms.First(r => r.Name.Contains("C"));
            var equipmentForC = allEquipment.Where(e => 
                e.Name == "TV" || e.Name == "WiFi" || e.Name == "Klimatyzacja").ToList();
            foreach (var eq in equipmentForC)
            {
                context.RoomEquipments.Add(new RoomEquipment
                {
                    RoomId = roomC.Id,
                    EquipmentId = eq.Id,
                    Quantity = 1
                });
            }

            // Board Room
            var boardRoom = allRooms.First(r => r.Name.Contains("Zarządu"));
            foreach (var eq in allEquipment)
            {
                context.RoomEquipments.Add(new RoomEquipment
                {
                    RoomId = boardRoom.Id,
                    EquipmentId = eq.Id,
                    Quantity = eq.Name == "TV" ? 2 : 1
                });
            }

            await context.SaveChangesAsync();
        }

        // Seed sample reservations
        if (!context.Reservations.Any())
        {
            var user = await userManager.FindByEmailAsync(userEmail);
            var rooms = context.Rooms.ToList();

            if (user != null && rooms.Any())
            {
                var reservations = new List<Reservation>
                {
                    new()
                    {
                        Title = "Spotkanie zespołu projektowego",
                        Description = "Cotygodniowe spotkanie statusowe projektu X",
                        StartDate = DateTime.Today.AddDays(1).AddHours(9),
                        EndDate = DateTime.Today.AddDays(1).AddHours(11),
                        NumberOfAttendees = 8,
                        Status = ReservationStatus.Approved,
                        UserId = user.Id,
                        RoomId = rooms[0].Id,
                        TotalCost = 2 * rooms[0].PricePerHour,
                        CreatedAt = DateTime.Now.AddDays(-2)
                    },
                    new()
                    {
                        Title = "Szkolenie BHP",
                        Description = "Obowiązkowe szkolenie okresowe",
                        StartDate = DateTime.Today.AddDays(3).AddHours(10),
                        EndDate = DateTime.Today.AddDays(3).AddHours(14),
                        NumberOfAttendees = 20,
                        Status = ReservationStatus.Pending,
                        UserId = user.Id,
                        RoomId = rooms[0].Id,
                        TotalCost = 4 * rooms[0].PricePerHour,
                        CreatedAt = DateTime.Now.AddDays(-1)
                    },
                    new()
                    {
                        Title = "Prezentacja kwartalna",
                        Description = "Prezentacja wyników Q4 dla zarządu",
                        StartDate = DateTime.Today.AddDays(7).AddHours(14),
                        EndDate = DateTime.Today.AddDays(7).AddHours(16),
                        NumberOfAttendees = 15,
                        Status = ReservationStatus.Pending,
                        UserId = user.Id,
                        RoomId = rooms[1].Id,
                        TotalCost = 2 * rooms[1].PricePerHour,
                        CreatedAt = DateTime.Now
                    }
                };

                context.Reservations.AddRange(reservations);
                await context.SaveChangesAsync();
            }
        }
    }
}