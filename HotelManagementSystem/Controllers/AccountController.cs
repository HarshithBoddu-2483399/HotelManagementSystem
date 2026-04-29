using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HotelManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _accountService.AuthenticateAsync(username, password);

            if (user != null)
            {
                if (user.Role == "Guest")
                {
                    var guestCheck = await _accountService.GetGuestByIdAsync(user.UserId);
                    if (guestCheck != null && guestCheck.RequiresPasswordReset)
                    {
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
        [AllowAnonymous]
        public IActionResult LoggedOut() => View();

        [AllowAnonymous]
        public IActionResult AccessDenied() => View();

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> Register(GuestRegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.RegisterGuestAsync(model);
                if (result.IsSuccess)
                {
                    TempData["Success"] = "Registration successful! You can now log in.";
                    return RedirectToAction("Login");
                }

                ModelState.AddModelError(result.ErrorMessage.Contains("Email") ? "Email" : "Phone", result.ErrorMessage);
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
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForceChangePassword(string newPassword, string confirmPassword, string newRecoveryPin)
        {
            if (TempData["ResetGuestId"] == null) return RedirectToAction("Login");
            int guestId = (int)TempData["ResetGuestId"];

            if (newPassword.Length < 6 || newPassword != confirmPassword || string.IsNullOrEmpty(newRecoveryPin) || newRecoveryPin.Length != 4)
            {
                ViewBag.Error = "Please ensure passwords match and your PIN is exactly 4 digits.";
                TempData.Keep("ResetGuestId");
                return View();
            }

            var success = await _accountService.ForceChangePasswordAsync(guestId, newPassword, newRecoveryPin);
            if (success)
            {
                TempData["Success"] = "Account fully secured! Please log in with your new password.";
                return RedirectToAction("Login");
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckEmail(string email)
        {
            bool exists = await _accountService.CheckEmailExistsAsync(email);
            return Json(new { exists });
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> CheckPhone(string phone)
        {
            bool exists = await _accountService.CheckPhoneExistsAsync(phone);
            return Json(new { exists });
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(GuestForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var success = await _accountService.ResetForgotPasswordAsync(model);
                if (success)
                {
                    TempData["Success"] = "Password reset successfully! You can now log in.";
                    return RedirectToAction("Login");
                }
                ViewBag.Error = "Verification failed. Please check your Email, Phone, or PIN. If you forgot your PIN, please contact the front desk.";
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ClaimAccount() => View();

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ClaimAccount(ClaimAccountViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _accountService.ClaimAccountAsync(model);
                if (result.IsSuccess)
                {
                    TempData["Success"] = "Account claimed successfully! You can now log in with your new password.";
                    return RedirectToAction("Login");
                }
                ViewBag.Error = result.ErrorMessage;
            }
            return View(model);
        }
    }
}