using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering; 
using HotelManagementSystem.Data;
using HotelManagementSystem.ViewModels;
using System.Linq;
using System.Security.Claims;
using System;

namespace HotelManagementSystem.Controllers
{
    public class HousekeepingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HousekeepingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult Index()
        {
            var pending = _context.HousekeepingTasks
                                  .Where(t => t.TaskStatus != "COMPLETED")
                                  .OrderBy(t => t.TaskDate)
                                  .ToList();
            return View(pending);
        }

        [Authorize(Roles = "Admin,Manager")]
        public IActionResult AllTasks()
        {
            var allTasks = _context.HousekeepingTasks.OrderByDescending(t => t.TaskDate).ToList();
            return View(allTasks);
        }

        [Authorize(Roles = "Housekeeping")]
        public IActionResult StaffIndex()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");

            int currentUserId = int.Parse(userIdClaim);

            var myTasks = _context.HousekeepingTasks
                                  .Where(t => t.AssignedStaffId == currentUserId && t.TaskStatus == "ASSIGNED")
                                  .OrderBy(t => t.TaskDate)
                                  .ToList();

            return View(myTasks);
        }

        [HttpPost]
        [Authorize(Roles = "Housekeeping")]
        public IActionResult CompleteTask(int taskId)
        {
            var task = _context.HousekeepingTasks.Find(taskId);
            if (task != null && task.TaskStatus != "COMPLETED")
            {
                task.TaskStatus = "COMPLETED";
                task.CompletedAt = DateTime.Now;

                var room = _context.Rooms.Find(task.RoomId);
                if (room != null)
                {
                    room.Status = "AVAILABLE";
                }

                _context.SaveChanges();
            }
            return RedirectToAction("StaffIndex");
        }

        public IActionResult History()
        {
            bool isStaff = User.IsInRole("Housekeeping");
            int currentUserId = 0;

            if (isStaff)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdClaim)) currentUserId = int.Parse(userIdClaim);
            }

            var history = _context.HousekeepingTasks
                .Where(t => t.TaskStatus == "COMPLETED" && (!isStaff || t.AssignedStaffId == currentUserId))
                .OrderByDescending(t => t.CompletedAt ?? t.TaskDate)
                .ToList();

            return View(history);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> MarkClean(int taskId)
        {
            var task = _context.HousekeepingTasks.FirstOrDefault(t => t.TaskId == taskId);

            if (task == null)
            {
                return NotFound();
            }

            task.TaskStatus = "COMPLETED";
            task.CompletedAt = DateTime.Now;

            var room = _context.Rooms.FirstOrDefault(r => r.RoomId == task.RoomId);
            if (room != null)
            {
                room.Status = "AVAILABLE";
            }

            _context.SaveChanges();

            return RedirectToAction("AllTasks");
        }

        [Authorize(Roles = "Admin,Manager,Housekeeping")]
        public IActionResult PerformanceReport(DateTime? startDate, DateTime? endDate, int? staffId)
        {
            bool isManagement = User.IsInRole("Admin") || User.IsInRole("Manager");
            int? targetStaffId = null;

            if (!isManagement)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim)) return RedirectToAction("Login", "Account");
                targetStaffId = int.Parse(userIdClaim);
            }
            else
            {
                targetStaffId = staffId;

                var activeStaffIds = _context.HousekeepingTasks
                                             .Where(t => t.AssignedStaffId != 0)
                                             .Select(t => t.AssignedStaffId)
                                             .Distinct()
                                             .ToList();

                ViewBag.StaffList = activeStaffIds.Select(id => new SelectListItem
                {
                    Value = id.ToString(),
                    Text = $"Staff ID: {id}"
                }).ToList();
            }

            DateTime start = startDate ?? DateTime.Today;
            DateTime end = endDate ?? DateTime.Today;

            var reportData = new PerformanceReportViewModel
            {
                StartDate = start,
                EndDate = end,
                SelectedStaffId = targetStaffId,
                TotalAssigned = 0,
                CompletedOnTime = 0,
                CompletedDelayed = 0
            };

            if (targetStaffId.HasValue)
            {
                var tasks = _context.HousekeepingTasks
                    .Where(t => t.AssignedStaffId == targetStaffId.Value
                             && t.TaskDate.Date >= start.Date
                             && t.TaskDate.Date <= end.Date)
                    .ToList();

                reportData.TotalAssigned = tasks.Count;

                reportData.CompletedOnTime = tasks.Count(t => t.TaskStatus == "COMPLETED" &&
                                                     (t.CompletedAt == null || t.CompletedAt <= t.TaskDate));

                reportData.CompletedDelayed = tasks.Count(t => t.TaskStatus == "COMPLETED" &&
                                                      t.CompletedAt > t.TaskDate);
            }

            return View(reportData);
        }
    }
}