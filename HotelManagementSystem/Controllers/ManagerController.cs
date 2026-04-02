using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using System.Linq;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly IRoomService _roomService;
        private readonly IHousekeepingService _hkService;

        public ManagerController(IManagerService managerService, IRoomService roomService, IHousekeepingService hkService)
        {
            _managerService = managerService;
            _roomService = roomService;
            _hkService = hkService;
        }

        // Dashboard Home - Only Summary Data
        public IActionResult Index()
        {
            var data = _managerService.GetManagerDashboardData();
            return View(data);
        }

        // Dedicated Page for Room Inventory
        public IActionResult Rooms()
        {
            var rooms = _roomService.GetAllRooms();
            return View(rooms); // Pass IEnumerable<Room>
        }

        // Dedicated Page for Task Assignment
        public IActionResult Housekeeping()
        {
            var data = _managerService.GetManagerDashboardData();
            var vm = new ManagerDashboardViewModel
            {
                PendingTasksList = _hkService.GetPendingTasks().ToList(),
                AvailableStaff = data.AvailableStaff
            };
            return View(vm);
        }

        [HttpPost]
        public IActionResult AssignStaff(int taskId, int staffId)
        {
            _managerService.AssignStaffToTask(taskId, staffId);
            return RedirectToAction("Housekeeping");
        }

        [HttpPost]
        public IActionResult ToggleMaintenance(int roomId)
        {
            _roomService.ToggleMaintenance(roomId);
            return RedirectToAction("Rooms");
        }
    }
}