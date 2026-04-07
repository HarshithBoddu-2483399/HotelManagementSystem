using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Data;
using HotelManagementSystem.ViewModels;
using System.Linq;
using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Receptionist,Manager,Admin")]
    public class ReceptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReceptionController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- DASHBOARD ---
        public IActionResult Index()
        {
            DateTime today = DateTime.Today;
            var viewModel = new ReceptionDashboardViewModel();

            viewModel.Rooms = _context.Rooms.ToList();
            viewModel.AvailableRooms = viewModel.Rooms.Count(r => r.Status == "AVAILABLE");
            viewModel.SafeToBook = new Dictionary<int, bool>();

            try
            {
                viewModel.TotalArrivals = _context.Reservations.Count(r => r.CheckInDate.Date == today);
                viewModel.TotalDepartures = _context.Reservations.Count(r => r.CheckOutDate.Date == today);
                viewModel.StayOvers = _context.Reservations.Count(r => r.CheckInDate.Date < today && r.CheckOutDate.Date > today && r.ReservationStatus == "ACTIVE");

                // ADVANCED COLLISION LOGIC
                var pendingArrivals = _context.Reservations.Where(r => r.CheckInDate.Date == today && (r.ReservationStatus == "PENDING" || r.ReservationStatus == "BOOKED")).ToList();
                var availableRoomsByType = viewModel.Rooms.Where(r => r.Status == "AVAILABLE").GroupBy(r => r.RoomType).ToDictionary(g => g.Key, g => g.Count());

                var reservedRoomIds = pendingArrivals.Select(r => r.RoomId).ToList();
                var reservedTypes = viewModel.Rooms.Where(r => reservedRoomIds.Contains(r.RoomId)).GroupBy(r => r.RoomType).ToDictionary(g => g.Key, g => g.Count());

                foreach (var room in viewModel.Rooms)
                {
                    if (room.Status != "AVAILABLE")
                    {
                        viewModel.SafeToBook[room.RoomId] = false;
                        continue;
                    }

                    int pendingOfType = reservedTypes.ContainsKey(room.RoomType) ? reservedTypes[room.RoomType] : 0;
                    int availOfType = availableRoomsByType.ContainsKey(room.RoomType) ? availableRoomsByType[room.RoomType] : 0;

                    viewModel.SafeToBook[room.RoomId] = availOfType > pendingOfType;
                }
            }
            catch
            {
                viewModel.SafeToBook = viewModel.Rooms.ToDictionary(r => r.RoomId, r => true);
            }

            return View(viewModel);
        }

        // --- MANAGE ROOM (For Booked/Occupied Rooms) ---
        public IActionResult RoomDetails(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room == null) return NotFound();

            // FIXED CHRONOLOGICAL LOGIC:
            // 1. Only look at active/booked statuses.
            // 2. Ensure CheckOutDate is in the future (ignore past stays).
            // 3. OrderBy (Ascending) ensures the CURRENT or SOONEST guest is always pulled first.
            var activeRes = _context.Reservations
                .Where(r => r.RoomId == roomId &&
                           (r.ReservationStatus == "ACTIVE" || r.ReservationStatus == "BOOKED" || r.ReservationStatus == "CHECKED_IN" || r.ReservationStatus == "CHECKED-IN"))
                .Where(r => r.CheckOutDate >= DateTime.Today)
                .OrderBy(r => r.CheckInDate)
                .FirstOrDefault();

            var viewModel = new RoomDetailsViewModel { Room = room, Reservation = activeRes };

            if (activeRes != null)
            {
                try
                {
                    var guest = _context.Guests.Find(activeRes.GuestId);
                    viewModel.GuestName = guest?.Name ?? "Guest ID: " + activeRes.GuestId;
                    viewModel.GuestPhone = guest?.ContactInfo ?? "N/A";
                    viewModel.GuestEmail = guest?.Email ?? "N/A";
                }
                catch
                {
                    viewModel.GuestName = "Guest ID: " + activeRes.GuestId;
                }
            }
            return View(viewModel);
        }

        // --- ASSIGN ROOM (For Available Rooms) ---
        public IActionResult AssignRoom(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room == null) return NotFound();

            var today = DateTime.Today;
            var arrivals = _context.Reservations.Where(r => r.CheckInDate.Date == today && (r.ReservationStatus == "PENDING" || r.ReservationStatus == "BOOKED")).ToList();

            var matchingArrivals = new List<ReservationDetails>();
            foreach (var res in arrivals)
            {
                var bookedRoom = _context.Rooms.Find(res.RoomId);

                if (bookedRoom != null && bookedRoom.RoomType == room.RoomType)
                {
                    string gName = "Guest ID: " + res.GuestId;
                    try
                    {
                        var guest = _context.Guests.Find(res.GuestId);
                        if (guest != null) gName = guest.Name;
                    }
                    catch { }

                    matchingArrivals.Add(new ReservationDetails
                    {
                        ReservationId = res.ReservationId,
                        GuestName = gName,
                        CheckInDate = res.CheckInDate,
                        CheckOutDate = res.CheckOutDate
                    });
                }
            }

            return View(new AssignRoomViewModel { Room = room, MatchingArrivals = matchingArrivals });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Receptionist")]
        public IActionResult ForceResetGuestPassword(int guestId)
        {
            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                guest.Password = "Hotel@1234";
                guest.RequiresPasswordReset = true; // <-- Flip the switch!
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Password for {guest.Name} has been reset to: Hotel@1234";
            }
            return RedirectToAction("Index");
        }
    }
}