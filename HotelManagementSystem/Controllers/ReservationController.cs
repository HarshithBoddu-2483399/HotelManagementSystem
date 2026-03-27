using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class ReservationController : Controller
    {
        private readonly IReservationService _resService;
        private readonly IRoomService _roomService;

        public ReservationController(IReservationService resService, IRoomService roomService)
        {
            _resService = resService; _roomService = roomService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Rooms = _roomService.GetAvailableRooms();
            return View();
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
            return RedirectToAction("Index", "Report");
        }
    }
}