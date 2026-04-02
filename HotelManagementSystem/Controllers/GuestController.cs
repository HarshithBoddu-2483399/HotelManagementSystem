using Microsoft.AspNetCore.Mvc;
using System.Linq;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;

        public GuestController(IGuestService guestService)
        {
            _guestService = guestService;
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
    }
}