using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Models;
using System;
using System.Linq;
using BCrypt.Net;
using HotelManagementSystem.Data;

namespace HotelManagementSystem.Controllers
{
    // SECURE: Only authorized users can access the directory
    [Authorize(Roles = "Admin")]
    public class StaffDirectoryController : Controller
    {
        private readonly IManagerService _managerService;
        private readonly ApplicationDbContext _context;

        public StaffDirectoryController(IManagerService managerService, ApplicationDbContext context)
        {
            _managerService = managerService;
            _context = context;
        }

        [HttpGet]
        public IActionResult StaffList()
        {
            var staff = _managerService.GetAllStaff();
            return View(staff);
        }

        [HttpGet]
        public IActionResult AddStaff()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddStaff(User staff)
        {
            if (ModelState.IsValid)
            {
                // SECURE: Automatically hash their starting password
                staff.Password = BCrypt.Net.BCrypt.HashPassword(staff.Password);

                _context.Users.Add(staff);
                _context.SaveChanges();

                TempData["Success"] = $"{staff.Name} has been successfully added to the staff directory.";
                return RedirectToAction("StaffList");
            }
            return View(staff);
        }

        [HttpGet]
        public IActionResult EditStaff(int userId)
        {
            var staff = _managerService.GetStaffById(userId);
            if (staff == null)
            {
                return RedirectToAction("StaffList");
            }
            return View(staff);
        }

        [HttpPost]
        public IActionResult EditStaff(User staff)
        {
            if (ModelState.IsValid)
            {
                _managerService.UpdateStaff(staff);
                TempData["Success"] = "Staff details updated successfully.";
                return RedirectToAction("StaffList");
            }
            return View(staff);
        }

        [HttpPost]
        public IActionResult DeleteStaff(int userId)
        {
            _managerService.DeleteStaff(userId);
            TempData["Success"] = "Staff member removed from the system.";
            return RedirectToAction("StaffList");
        }

        // ==========================================
        // DAILY ATTENDANCE
        // ==========================================

        [HttpGet]
        public IActionResult Attendance(DateTime? date = null)
        {
            var selectedDate = date ?? DateTime.Today;
            var data = _managerService.GetAttendanceByDate(selectedDate);

            ViewBag.SelectedDate = selectedDate;
            return View(data);
        }

        [HttpPost]
        public IActionResult MarkAttendance(int userId, DateTime selectedDate, bool isPresent)
        {
            if (selectedDate.Date != DateTime.Today)
            {
                TempData["Error"] = "You can only mark attendance for today's date.";
                return RedirectToAction("Attendance", new { date = selectedDate.ToString("yyyy-MM-dd") });
            }

            _managerService.MarkAttendance(userId, selectedDate, isPresent);
            return RedirectToAction("Attendance", new { date = selectedDate.ToString("yyyy-MM-dd") });
        }
    }
}