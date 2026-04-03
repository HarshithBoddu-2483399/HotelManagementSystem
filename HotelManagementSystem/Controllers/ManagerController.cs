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

        // --- DASHBOARD & ROOMS ---
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

        // --- HOUSEKEEPING ASSIGNMENTS ---
        [HttpGet]
        public IActionResult Housekeeping()
        {
            // Sends only Housekeeping staff to the View dropdown
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

        // --- STAFF MANAGEMENT ---
        [HttpGet]
        public IActionResult StaffList()
        {
            return View(_managerService.GetAllStaff());
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStaff(User staff)
        {
            _managerService.AddStaff(staff);
            return RedirectToAction("StaffList");
        }

        [HttpGet]
        public IActionResult EditStaff(int userId)
        {
            var staff = _managerService.GetStaffById(userId);
            if (staff == null) return RedirectToAction("StaffList");
            return View(staff);
        }

        [HttpPost]
        public IActionResult EditStaff(User staff)
        {
            _managerService.UpdateStaff(staff);
            return RedirectToAction("StaffList");
        }

        [HttpPost]
        public IActionResult DeleteStaff(int userId)
        {
            _managerService.DeleteStaff(userId);
            return RedirectToAction("StaffList");
        }

        // --- ATTENDANCE ---
        [HttpGet]
        public IActionResult Attendance(DateTime? date = null)
        {
            var selectedDate = date ?? DateTime.Today;
            var data = _managerService.GetAttendanceByDate(selectedDate);
            return View(data);
        }

        [HttpPost]
        public IActionResult MarkAttendance(int userId, DateTime selectedDate, bool isPresent)
        {
            if (selectedDate.Date != DateTime.Today)
            {
                return RedirectToAction("Attendance", new { date = selectedDate.ToString("yyyy-MM-dd") });
            }

            _managerService.MarkAttendance(userId, selectedDate, isPresent);
            return RedirectToAction("Attendance", new { date = selectedDate.ToString("yyyy-MM-dd") });
        }
    }
}