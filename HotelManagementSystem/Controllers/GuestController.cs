using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.Data;   
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace HotelManagementSystem.Controllers
{
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;
        private readonly ApplicationDbContext _context;

        public GuestController(IGuestService guestService, ApplicationDbContext context)
        {
            _guestService = guestService;
            _context = context;
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
            var result = _guestService.CreateGuest(guest);

            if (!result.IsSuccess)
            {
                ViewBag.Error = result.ErrorMessage;
                return View(guest);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var guests = _guestService.GetAllGuests();
            return View(guests);
        }

        [HttpGet]
        public IActionResult FindByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest();
            }

            var matches = _guestService.FindByPhone(phone)
                .Select(g => new { name = g.Name, email = g.Email, phone = g.ContactInfo })
                .ToList();

            return Json(matches);
        }

        [HttpGet]
        public IActionResult FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest();
            }

            var match = _guestService.FindByEmail(email);
            if (match != null)
            {
                return Json(new { name = match.Name, email = match.Email, phone = match.ContactInfo });
            }

            return Json(null);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Receptionist")] // Only staff can do this!
        public IActionResult ForceResetGuestPassword(int guestId)
        {
            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                // 1. Change password to default
                guest.Password = "Hotel@1234";

                // 2. Flip the switch so they MUST change it on their next login
                guest.RequiresPasswordReset = true;

                _context.SaveChanges();

                TempData["Success"] = $"Success! Password for {guest.Name} has been reset to: Hotel@1234";
            }

            // Refresh the guest list page
            return RedirectToAction("Index");
        }
    }
}