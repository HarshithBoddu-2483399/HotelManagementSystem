using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        private readonly IGuestService _guestService;
        private readonly ApplicationDbContext _context; 

        public ReservationController(IReservationService resService, IRoomService roomService, IGuestService guestService, ApplicationDbContext context)
        {
            _resService = resService;
            _roomService = roomService;
            _guestService = guestService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Create(int? roomId)
        {
            var allRooms = _context.Rooms.ToList();
            ViewBag.Rooms = allRooms;
            ViewBag.SelectedRoomId = roomId;

            if (roomId.HasValue)
            {
                var selectedRoom = allRooms.FirstOrDefault(r => r.RoomId == roomId.Value);
                if (selectedRoom != null)
                {
                    ViewBag.PreselectedRoomType = selectedRoom.RoomType;
                }
            }

            return View();
        }

        [HttpGet]
        public IActionResult CheckAvailability(int roomId, DateTime checkIn, DateTime checkOut)
        {
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
                    message = $"⚠️ This room is blocked until {availableTime:MMM dd, hh:mm tt} for the previous guest and housekeeping. Please select a later time or a different room."
                });
            }

            return Json(new { available = true });
        }

        [HttpGet]
        public IActionResult Index()
        {
            var all = _resService.GetAllReservations();
            return View(all);
        }

        [HttpPost]
        public IActionResult Create(Reservation reservation, Guest guest)
        {
            reservation.ReservationStatus = "BOOKED";

            if (!_resService.CreateReservation(reservation, guest))
            {
                ViewBag.Error = "Room unavailable for these specific times.";
                ViewBag.Rooms = _context.Rooms.ToList();
                ViewBag.SelectedRoomId = reservation.RoomId;
                return View();
            }

            return RedirectToAction("Index", "Billing");
        }
    }
}