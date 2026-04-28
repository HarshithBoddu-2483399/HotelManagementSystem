using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;
using HotelManagementSystem.Data;
using System.Linq;
using System;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class ReservationController : Controller
    {
        private readonly IReservationService _resService;
        private readonly IRoomService _roomService;
        private readonly ApplicationDbContext _context;

        public ReservationController(IReservationService resService, IRoomService roomService, ApplicationDbContext context)
        {
            _resService = resService;
            _roomService = roomService;
            _context = context;
        }

        // GET: Displays the booking form
        [HttpGet]
        public IActionResult Create(int? roomId)
        {
            // Filter: Only show rooms that are AVAILABLE (not in MAINTENANCE or BOOKED)
            var availableRooms = _context.Rooms.Where(r => r.Status == "AVAILABLE").ToList();
            ViewBag.Rooms = availableRooms;
            ViewBag.SelectedRoomId = roomId;

            if (roomId.HasValue)
            {
                var selectedRoom = availableRooms.FirstOrDefault(r => r.RoomId == roomId.Value);
                if (selectedRoom != null)
                {
                    ViewBag.PreselectedRoomType = selectedRoom.RoomType;
                }
            }

            return View();
        }

        // POST: Handles the actual booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Reservation reservation, Guest guest)
        {
            // Re-check status on server to prevent bypass
            var room = _context.Rooms.Find(reservation.RoomId);

            if (room == null || room.Status == "MAINTENANCE")
            {
                ViewBag.Error = "This room is currently under maintenance and cannot be booked.";
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "AVAILABLE").ToList();
                return View();
            }

            reservation.ReservationStatus = "BOOKED";

            if (!_resService.CreateReservation(reservation, guest))
            {
                ViewBag.Error = "Room unavailable for these specific times.";
                ViewBag.Rooms = _context.Rooms.Where(r => r.Status == "AVAILABLE").ToList();
                return View();
            }

            return RedirectToAction("Index", "Billing");
        }

        [HttpGet]
        public IActionResult CheckAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
            var room = _context.Rooms.Find(roomId);

            // Check status string instead of boolean
            if (room != null && room.Status == "MAINTENANCE")
            {
                return Json(new
                {
                    available = false,
                    message = "🛠️ This room is under maintenance and cannot be reserved."
                });
            }

            var overlap = _context.Reservations.Where(r =>
                r.RoomId == roomId &&
                r.ReservationStatus != "CANCELLED" &&
                r.ReservationStatus != "COMPLETED" &&
                checkIn < r.CheckOutDate.AddHours(1) &&
                checkOut > r.CheckInDate)
            .OrderByDescending(r => r.CheckOutDate)
            .FirstOrDefault();

            if (overlap != null)
            {
                DateTime availableTime = overlap.CheckOutDate.AddHours(1);
                return Json(new
                {
                    available = false,
                    message = $"⚠️ Blocked until {availableTime:MMM dd, hh:mm tt} for previous guest/cleaning."
                });
            }

            return Json(new { available = true });
        }

        public IActionResult Index()
        {
            var all = _resService.GetAllReservations();
            return View(all);
        }
    }
}