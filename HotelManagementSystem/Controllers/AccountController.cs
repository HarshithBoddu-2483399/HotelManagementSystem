using HotelManagementSystem.Data;
using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ApplicationDbContext _context;

        public AccountController(IAccountService accountService , ApplicationDbContext context)
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // 1. This one LOADS the page
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // 2. This one CATCHES the form submission (The 405 fix)
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
                    Password = model.Password,
                    RecoveryPin = model.RecoveryPin
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
            // If they just typed the URL without actually logging in, kick them out
            if (TempData["ResetGuestId"] == null) return RedirectToAction("Login");

            // Keep the ID alive for the POST request
            TempData.Keep("ResetGuestId");
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ForceChangePassword(string newPassword, string confirmPassword)
        {
            if (TempData["ResetGuestId"] == null) return RedirectToAction("Login");
            int guestId = (int)TempData["ResetGuestId"];

            if (newPassword.Length < 6)
            {
                ViewBag.Error = "Password must be at least 6 characters.";
                TempData.Keep("ResetGuestId");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                TempData.Keep("ResetGuestId");
                return View();
            }

            var guest = _context.Guests.Find(guestId);
            if (guest != null)
            {
                guest.Password = newPassword;
                guest.RequiresPasswordReset = false; // Turn the switch back off!
                _context.SaveChanges();

                TempData["Success"] = "Password successfully updated! Please log in with your new password.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }

        // --- LIVE AJAX VALIDATION APIs ---
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
                var guest = _context.Guests.FirstOrDefault(g =>
                    g.Email == model.Email &&
                    g.ContactInfo == model.Phone &&
                    g.RecoveryPin == model.RecoveryPin);

                if (guest != null)
                {
                    guest.Password = model.NewPassword;
                    _context.SaveChanges();
                    TempData["Success"] = "Password reset successfully! You can now log in.";
                    return RedirectToAction("Login");
                }

                ViewBag.Error = "Verification failed. Please check your Email, Phone, or PIN. If you forgot your PIN, please contact the front desk.";
            }
            return View(model);
        }

        // --- CLAIM ACCOUNT / FORGOT PASSWORD ---
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
                // 1. Find the reservation they are claiming
                var reservation = _context.Reservations.Find(model.ReservationId);

                if (reservation != null)
                {
                    // 2. Find the guest attached to this exact reservation
                    var guest = _context.Guests.Find(reservation.GuestId);

                    // 3. Verify Email and Phone match EXACTLY (Security Check)
                    if (guest != null &&
                        guest.Email.ToLower().Trim() == model.Email.ToLower().Trim() &&
                        guest.ContactInfo.Trim() == model.Phone.Trim())
                    {
                        // 4. Update the password
                        guest.Password = model.NewPassword;
                        _context.SaveChanges();

                        // Send success message to the Login page
                        TempData["Success"] = "Account claimed successfully! You can now log in with your new password.";
                        return RedirectToAction("Login");
                    }
                }

                // Generic error for security (so people can't easily guess valid Reservation IDs)
                ViewBag.Error = "Verification failed. Please ensure your Reservation ID, Email, and Phone match your booking exactly.";
            }

            return View(model);
        }
    }
}