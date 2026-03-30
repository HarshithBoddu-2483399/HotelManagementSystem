using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class GuestController : Controller
    {
        private readonly IGuestService _guestService;
        public GuestController(IGuestService guestService) { _guestService = guestService; }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Guest guest)
        {
            if (guest == null || string.IsNullOrEmpty(guest.Email))
            {
                ViewBag.Error = "Please provide guest details.";
                return View();
            }

            _guestService.CreateGuest(guest);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var guests = _guestService.GetAllGuests();
            return View(guests);
        }
    }
}