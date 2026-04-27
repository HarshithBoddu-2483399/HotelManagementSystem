using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        public IActionResult Index()
        {
            var rooms = _roomService.GetAllRooms();
            return View(rooms);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Room room)
        {
            _roomService.AddRoom(room);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var room = _roomService.GetRoomById(id);
            if (room == null) return View("NotFound");
            return View(room);
        }

        [HttpPost]
        public IActionResult Edit(Room room)
        {
            var existing = _roomService.GetRoomById(room.RoomId);
            if (existing == null) return View("NotFound");

            _roomService.UpdateRoom(room);
            return RedirectToAction("Index");
        }

        // UPDATED: Handle the Tuple result and set TempData
        [HttpPost]
        public IActionResult ToggleMaintenance(int id)
        {
            var result = _roomService.ToggleMaintenance(id);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction("Index");
        }
    }
}