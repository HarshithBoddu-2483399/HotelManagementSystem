using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using System.Linq;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class GuestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var guests = _context.Guests.ToList();
            return View(guests);
        }

        // ==========================================
        // NEW: API for Phone Lookup (Used by Booking & Register pages)
        // ==========================================
        [HttpGet]
        public IActionResult FindByPhone(string phone)
        {
            if (string.IsNullOrEmpty(phone)) return Json(new object[] { });

            // Search for guests matching this phone number
            var matches = _context.Guests
                .Where(g => g.ContactInfo.Contains(phone))
                .Select(g => new {
                    id = g.GuestId,
                    name = g.Name,
                    email = g.Email,
                    phone = g.ContactInfo
                })
                .ToList();

            return Json(matches);
        }

        // ==========================================
        // NEW: API for Email Lookup (Used by Register page)
        // ==========================================
        [HttpGet]
        public IActionResult FindByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return Json(null);

            var guest = _context.Guests
                .Where(g => g.Email == email)
                .Select(g => new {
                    id = g.GuestId,
                    name = g.Name,
                    email = g.Email,
                    phone = g.ContactInfo
                })
                .FirstOrDefault();

            return Json(guest);
        }

        [HttpGet]
        public IActionResult Create(string phone = null)
        {
            var guest = new Guest();
            if (!string.IsNullOrEmpty(phone))
            {
                guest.ContactInfo = phone;
            }
            return View(guest);
        }

        [HttpPost]
        public IActionResult Create(Guest guest)
        {
            if (ModelState.IsValid)
            {
                // Strict duplicate check before saving
                if (_context.Guests.Any(g => g.Email == guest.Email || g.ContactInfo == guest.ContactInfo))
                {
                    ViewBag.Error = "This guest is already registered in the system.";
                    return View(guest);
                }

                guest.Password = BCrypt.Net.BCrypt.HashPassword("Welcome@123");
                guest.RecoveryPin = BCrypt.Net.BCrypt.HashPassword("1234");
                guest.RequiresPasswordReset = true;

                _context.Guests.Add(guest);
                _context.SaveChanges();

                TempData["Success"] = "Guest registered successfully!";
                return RedirectToAction("Index");
            }
            return View(guest);
        }

        [HttpPost]
        public IActionResult ForceResetGuestPassword(int guestId)
        {
            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                guest.Password = BCrypt.Net.BCrypt.HashPassword("Hotel@1234");
                guest.RequiresPasswordReset = true;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}