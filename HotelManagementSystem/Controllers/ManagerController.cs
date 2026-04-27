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
        // ADDED: IRoomService to share the strict validation logic
        private readonly IRoomService _roomService;

        public ManagerController(IManagerService managerService, ApplicationDbContext context, IRoomService roomService)
        {
            _managerService = managerService;
            _context = context;
            _roomService = roomService;
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
            // UPDATED: Use the secure RoomService logic instead of direct context updates
            var result = _roomService.ToggleMaintenance(roomId);

            if (!result.Success)
            {
                // Send the error message to the view to trigger the popup
                TempData["ErrorMessage"] = result.Message;
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