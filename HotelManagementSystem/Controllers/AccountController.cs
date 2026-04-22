using HotelManagementSystem.Data;
using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ApplicationDbContext _context;

        public AccountController(IAccountService accountService, ApplicationDbContext context)
        {
            _accountService = accountService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _accountService.Authenticate(username, password);

            if (user != null)
            {
                if (user.Role == "Guest")
                {
                    var guestCheck = _context.Guests.Find(user.UserId);
                    if (guestCheck != null && guestCheck.RequiresPasswordReset)
                    {
                        // Store their ID securely for one single request
                        TempData["ResetGuestId"] = guestCheck.GuestId;
                        return RedirectToAction("ForceChangePassword");
                    }
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString())
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                return user.Role switch
                {
                    "Admin" => RedirectToAction("Index", "Report"),
                    "Manager" => RedirectToAction("Index", "Manager"),  
                    "Housekeeping" => RedirectToAction("StaffIndex", "Housekeeping"),
                    "Receptionist" => RedirectToAction("Index", "Reception"),
                    "Guest" => RedirectToAction("Index", "GuestPortal"),
                    _ => RedirectToAction("Index", "Billing")
                };
            }

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("LoggedOut");
        }

        [HttpGet]
        [AllowAnonymous] // Anyone can see this page, even if logged out
        public IActionResult LoggedOut()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult Register(ViewModels.GuestRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                if (_context.Guests.Any(g => g.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Already user with this mail id exists.");
                    return View(model);
                }
                if (_context.Guests.Any(g => g.ContactInfo == model.Phone))
                {
                    ModelState.AddModelError("Phone", "Phone no you are trying has already acc with us trying resetting password.");
                    return View(model);
                }

                var newGuest = new Models.Guest
                {
                    Name = model.Name,
                    Email = model.Email,
                    ContactInfo = model.Phone,
                    // SECURE: Both Password and PIN are securely hashed
                    Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RecoveryPin = BCrypt.Net.BCrypt.HashPassword(model.RecoveryPin)
                };

                _context.Guests.Add(newGuest);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful! You can now log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForceChangePassword()
        {
            if (TempData["ResetGuestId"] == null) return RedirectToAction("Login");
            TempData.Keep("ResetGuestId");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        // UPDATED: Added newRecoveryPin to parameters so they can set a new secure PIN after a reset
        public IActionResult ForceChangePassword(string newPassword, string confirmPassword, string newRecoveryPin)
        {
            if (TempData["ResetGuestId"] == null) return RedirectToAction("Login");
            int guestId = (int)TempData["ResetGuestId"];

            // Added check for the PIN length
            if (newPassword.Length < 6 || newPassword != confirmPassword || string.IsNullOrEmpty(newRecoveryPin) || newRecoveryPin.Length != 4)
            {
                ViewBag.Error = "Please ensure passwords match and your PIN is exactly 4 digits.";
                TempData.Keep("ResetGuestId");
                return View();
            }

            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                // SECURE: Hash both the new Password and the new PIN
                guest.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                guest.RecoveryPin = BCrypt.Net.BCrypt.HashPassword(newRecoveryPin);
                guest.RequiresPasswordReset = false;
                _context.SaveChanges();

                TempData["Success"] = "Account fully secured! Please log in with your new password.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CheckEmail(string email)
        {
            bool exists = _context.Guests.Any(g => g.Email == email);
            return Json(new { exists });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult CheckPhone(string phone)
        {
            bool exists = _context.Guests.Any(g => g.ContactInfo == phone);
            return Json(new { exists });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ForgotPassword(ViewModels.GuestForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                // SECURE CHECK 1: Only find by Email and Phone (Ignore PIN for now since it's hashed in DB)
                var guest = _context.Guests.FirstOrDefault(g =>
                    g.Email == model.Email &&
                    g.ContactInfo == model.Phone);

                if (guest != null && !string.IsNullOrEmpty(guest.RecoveryPin))
                {
                    // SECURE CHECK 2: Verify the typed PIN against the hashed PIN in the DB
                    if (BCrypt.Net.BCrypt.Verify(model.RecoveryPin, guest.RecoveryPin))
                    {
                        // SECURE: Hash the new password before saving
                        guest.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        _context.SaveChanges();

                        TempData["Success"] = "Password reset successfully! You can now log in.";
                        return RedirectToAction("Login");
                    }
                }

                ViewBag.Error = "Verification failed. Please check your Email, Phone, or PIN. If you forgot your PIN, please contact the front desk.";
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ClaimAccount()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ClaimAccount(ViewModels.ClaimAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var reservation = _context.Reservations.Find(model.ReservationId);

                if (reservation != null)
                {
                    var guest = _context.Guests.Find(reservation.GuestId);

                    if (guest != null &&
                        guest.Email.ToLower().Trim() == model.Email.ToLower().Trim() &&
                        guest.ContactInfo.Trim() == model.Phone.Trim())
                    {
                        // SECURE: Hash BOTH the new Password and the new PIN when claiming
                        guest.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
                        guest.RecoveryPin = BCrypt.Net.BCrypt.HashPassword(model.RecoveryPin);
                        _context.SaveChanges();

                        TempData["Success"] = "Account claimed successfully! You can now log in with your new password.";
                        return RedirectToAction("Login");
                    }
                }

                ViewBag.Error = "Verification failed. Please ensure your Reservation ID, Email, and Phone match your booking exactly.";
            }

            return View(model);
        }
    }
}