using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using HotelManagementSystem.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HotelManagementSystem.Controllers
{
    public class GuestAuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GuestAuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // If they are already logged in as a guest, send them straight to the portal
            if (User.Identity.IsAuthenticated && User.IsInRole("Guest"))
            {
                return RedirectToAction("Index", "GuestPortal");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string phone)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone))
            {
                ViewBag.Error = "Please provide both Email and Phone number.";
                return View();
            }

            // Find the guest matching the Email and ContactInfo (Phone)
            var guest = _context.Guests.FirstOrDefault(g => g.Email == email && g.ContactInfo == phone);

            if (guest != null)
            {
                // Create the secure identity claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, guest.Name),
                    new Claim(ClaimTypes.Role, "Guest"), // Assign the Guest role
                    new Claim("GuestId", guest.GuestId.ToString()) // Store their specific ID
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Sign the user in
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // Route them directly to the Guest Portal
                return RedirectToAction("Index", "GuestPortal");
            }

            ViewBag.Error = "We couldn't find a reservation with that Email and Phone combination.";
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}