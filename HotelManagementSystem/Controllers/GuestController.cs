using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Data;
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

        [HttpPost]
        public IActionResult ForceResetGuestPassword(int guestId)
        {
            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                // 1. Hash the default temporary password
                guest.Password = BCrypt.Net.BCrypt.HashPassword("Hotel@1234");

                // 2. Flip the switch so they MUST change it on their next login
                guest.RequiresPasswordReset = true;

                _context.SaveChanges();

                TempData["Success"] = $"Success! Password for {guest.Name} has been reset to: Hotel@1234";
            }

            return RedirectToAction("Index");
        }
    }
}