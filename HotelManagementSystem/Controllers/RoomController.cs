using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
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
        public IActionResult Create()
        {
            return View();
        }

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

        [HttpPost]
        public IActionResult ToggleMaintenance(int id)
        {
            _roomService.ToggleMaintenance(id);
            return RedirectToAction("Index");
        }
    }
}