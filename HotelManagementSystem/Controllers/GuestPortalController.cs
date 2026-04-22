using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Data;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Models;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Security.Claims;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Guest")]
    public class GuestPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestPortalController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var guestIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(guestIdClaim) || !int.TryParse(guestIdClaim, out int loggedInGuestId))
            {
                return RedirectToAction("Login", "Account");
            }

            var guest = _context.Guests.Find(loggedInGuestId);
            if (guest == null) return NotFound("Guest profile not found.");

            var allReservations = _context.Reservations
                .Where(r => r.GuestId == loggedInGuestId)
                .OrderByDescending(r => r.CheckInDate)
                .ToList();

            var activeList = new List<GuestBookingInfo>();
            var pastList = new List<GuestBookingInfo>();

            foreach (var res in allReservations)
            {
                var room = _context.Rooms.Find(res.RoomId);
                var inv = _context.Invoices.FirstOrDefault(i => i.ReservationId == res.ReservationId);

                var info = new GuestBookingInfo
                {
                    ReservationId = res.ReservationId,
                    RoomNumber = room?.RoomNumber ?? "N/A",
                    RoomType = room?.RoomType ?? "N/A",
                    CheckIn = res.CheckInDate,
                    CheckOut = res.CheckOutDate,
                    Status = res.ReservationStatus,
                    TotalAmount = inv?.TotalAmount ?? 0m,
                    IsPaid = inv?.PaymentStatus == "PAID",
                    InvoiceId = inv?.InvoiceId
                };

                if (res.ReservationStatus == "COMPLETED" ||
                    res.ReservationStatus == "CANCELLED" ||
                    res.ReservationStatus == "CHECKED_OUT" ||
                    res.ReservationStatus == "CHECKED-OUT" ||
                    res.ReservationStatus == "INVOICED")
                {
                    pastList.Add(info);
                }
                else
                {
                    activeList.Add(info);
                }
            }

            var viewModel = new GuestDashboardViewModel
            {
                Guest = guest,
                ActiveBookings = activeList,
                PastBookings = pastList
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult BookRoom()
        {
            ViewBag.RoomTypes = _context.Rooms.Select(r => r.RoomType).Distinct().ToList();
            return View();
        }

        [HttpGet]
        public IActionResult CheckTypeAvailability(string roomType, DateTime checkIn, DateTime checkOut)
        {
            var matchingRooms = _context.Rooms.Where(r => r.RoomType == roomType).ToList();

            foreach (var room in matchingRooms)
            {
                bool isOverlap = _context.Reservations.Any(r =>
                    r.RoomId == room.RoomId &&
                    r.ReservationStatus != "CANCELLED" &&
                    r.ReservationStatus != "COMPLETED" &&
                    checkIn < r.CheckOutDate.AddHours(1) &&
                    checkOut > r.CheckInDate);

                if (!isOverlap)
                {
                    return Json(new { available = true, roomId = room.RoomId, rate = room.RatePerNight });
                }
            }

            return Json(new { available = false, message = $"⚠️ Sorry, all '{roomType}' rooms are fully booked for these specific dates/times." });
        }

        [HttpPost]
        public IActionResult SubmitBooking(int roomId, DateTime checkInDate, DateTime checkOutDate)
        {
            var guestIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(guestIdClaim) || !int.TryParse(guestIdClaim, out int loggedInGuestId))
                return RedirectToAction("Login", "Account");

            var reservation = new Reservation
            {
                GuestId = loggedInGuestId,
                RoomId = roomId,
                CheckInDate = checkInDate,
                CheckOutDate = checkOutDate,
                ReservationStatus = "BOOKED"
            };

            var room = _context.Rooms.Find(roomId);
            if (room != null && room.Status == "AVAILABLE")
            {
                room.Status = "BOOKED";
            }

            _context.Reservations.Add(reservation);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // CANCEL BOOKING
        [HttpPost]
        public IActionResult CancelBooking(int reservationId)
        {
            var guestIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(guestIdClaim) || !int.TryParse(guestIdClaim, out int loggedInGuestId))
                return RedirectToAction("Login", "Account");

            var res = _context.Reservations.Find(reservationId);

            if (res != null && res.GuestId == loggedInGuestId && res.ReservationStatus == "BOOKED")
            {
                res.ReservationStatus = "CANCELLED";

                var room = _context.Rooms.Find(res.RoomId);
                if (room != null && room.Status == "BOOKED")
                {
                    room.Status = "AVAILABLE";
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        // --- SECURE GUEST RECEIPT VIEW ---
        [HttpGet]
        public IActionResult ViewReceipt(int reservationId)
        {
            var guestIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(guestIdClaim) || !int.TryParse(guestIdClaim, out int loggedInGuestId))
                return RedirectToAction("Login", "Account");

            var res = _context.Reservations.Find(reservationId);

            if (res == null || res.GuestId != loggedInGuestId)
                return Content("Access Denied: You do not have permission to view this receipt.");

            var invoice = _context.Invoices.FirstOrDefault(i => i.ReservationId == reservationId);
            if (invoice == null)
                return Content("Receipt has not been generated yet.");

            ViewBag.Reservation = res;
            ViewBag.Room = _context.Rooms.Find(res.RoomId);
            ViewBag.Guest = _context.Guests.Find(res.GuestId);

            return View(invoice);
        }
    }
}