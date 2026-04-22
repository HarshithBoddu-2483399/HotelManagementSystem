using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;
using System;
using System.Linq;
using HotelManagementSystem.Data;

namespace HotelManagementSystem.Controllers
{
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
            // STRICT VALIDATION: Ensure the assigned time is not past the 1-hour grace period
            var task = _context.HousekeepingTasks.Find(taskId);
            if (task != null && task.CheckoutTime.HasValue)
            {
                DateTime requiredDeadline = task.CheckoutTime.Value.AddHours(1);

                // Combine the Manager's submitted Date and Time to check the exact moment
                DateTime assignedDateTime = targetDate.Date.Add(deadlineTime);

                if (assignedDateTime > requiredDeadline)
                {
                    TempData["ErrorMessage"] = $"Assignment blocked: Room must be cleaned by {requiredDeadline:dd MMM yyyy, hh:mm tt}.";
                    return RedirectToAction("Housekeeping");
                }
            }

            _managerService.AssignStaffToTask(taskId, staffId, targetDate, deadlineTime);
            TempData["SuccessMessage"] = "Staff assigned successfully.";
            return RedirectToAction("Housekeeping");
        }
    }
}