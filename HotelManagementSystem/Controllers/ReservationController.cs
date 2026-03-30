using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _resService;
        private readonly IRoomService _roomService;
        private readonly IGuestService _guestService;

        public ReservationController(IReservationService resService, IRoomService roomService, IGuestService guestService)
        {
            _resService = resService;
            _roomService = roomService;
            _guestService = guestService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Rooms = _roomService.GetAvailableRooms();
            return View();
        }

        [HttpGet]
        public IActionResult Index()
        {
            var all = _resService.GetAllReservations();
            return View(all);
        }

        [HttpPost]
        public IActionResult Create(Reservation reservation, Guest guest)
        {
            if (!_resService.CreateReservation(reservation, guest))
            {
                ViewBag.Error = "Room unavailable for these dates.";
                ViewBag.Rooms = _roomService.GetAvailableRooms();
                return View();
            }
            // After creating a reservation, redirect to Billing so the new booking appears in Active/Upcoming immediately
            return RedirectToAction("Index", "Billing");
        }
    }
}