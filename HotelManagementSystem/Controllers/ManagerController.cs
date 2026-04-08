using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;
using System;
using System.Linq;
using HotelManagementSystem.Data;

namespace HotelManagementSystem.Controllers
{
    // Keeping Admin here just in case the Admin wants to help the Manager assign rooms
    [Authorize(Roles = "Manager,Admin")]
    public class ManagerController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly ApplicationDbContext _context;

        public ManagerController(IManagerService managerService, ApplicationDbContext context)
        {
            _managerService = managerService;
            _context = context;
        }

        public IActionResult Index()
        {
            var data = _managerService.GetManagerDashboardData();
            return View(data);
        }

        public IActionResult Rooms()
        {
            var rooms = _context.Rooms.ToList();
            return View(rooms);
        }

        [HttpPost]
        public IActionResult ToggleMaintenance(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room != null)
            {
                room.Status = room.Status == "MAINTENANCE" ? "AVAILABLE" : "MAINTENANCE";
                _context.SaveChanges();
            }
            return RedirectToAction("Rooms");
        }

        [HttpGet]
        public IActionResult Housekeeping()
        {
            ViewBag.AvailableStaff = _managerService.GetAllStaff().Where(u => u.Role == "Housekeeping").ToList();
            var pendingTasks = _context.HousekeepingTasks.Where(t => t.TaskStatus == "PENDING").ToList();
            return View(pendingTasks);
        }

        [HttpPost]
        public IActionResult AssignStaffToTask(int taskId, int staffId, DateTime targetDate, TimeSpan deadlineTime)
        {
            _managerService.AssignStaffToTask(taskId, staffId, targetDate, deadlineTime);
            return RedirectToAction("Housekeeping");
        }
    }
}