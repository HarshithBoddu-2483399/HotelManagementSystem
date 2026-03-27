using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Controllers
{
    public class RoomController : Controller
    {
        private readonly IRoomService _roomService;
        public RoomController(IRoomService roomService) { _roomService = roomService; }

        public IActionResult Index() => View(_roomService.GetAllRooms());

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        public IActionResult Create(Room room)
        {
            _roomService.AddRoom(room);
            return RedirectToAction("Index");
        }
    }
}