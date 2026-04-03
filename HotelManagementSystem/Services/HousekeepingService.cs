using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.Services
{
    public class HousekeepingService : IHousekeepingService
    {
        private readonly ApplicationDbContext _context;
        public HousekeepingService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<HousekeepingTask> GetPendingTasks() => _context.HousekeepingTasks.Where(t => t.TaskStatus == "PENDING").ToList();

        public IEnumerable<HousekeepingTask> GetAllTasks() => _context.HousekeepingTasks.OrderByDescending(t => t.TaskDate).ToList();

        public void MarkClean(int taskId)
        {
            var task = _context.HousekeepingTasks.Find(taskId);
            if (task != null)
            {
                task.TaskStatus = "COMPLETED";
                var room = _context.Rooms.Find(task.RoomId);
                if (room != null) room.Status = "AVAILABLE";
                _context.SaveChanges();
            }
        }

        public IEnumerable<HousekeepingTask> GetStaffTasks(int staffId)
        {
            return _context.HousekeepingTasks
                .Where(t => t.AssignedStaffId == staffId && t.TaskStatus != "COMPLETED")
                .OrderByDescending(t => t.TaskDate)
                .ToList();
        }

        public IEnumerable<HousekeepingTask> GetCompletedStaffTasks(int staffId)
        {
            return _context.HousekeepingTasks
                .Where(t => t.AssignedStaffId == staffId && t.TaskStatus == "COMPLETED")
                .OrderByDescending(t => t.TaskDate)
                .ToList();
        }

        public StaffPerformanceViewModel GetStaffPerformance(int staffId)
        {
            var today = System.DateTime.Today;

            var completedTasks = _context.HousekeepingTasks
                .Where(t => t.AssignedStaffId == staffId && t.TaskStatus == "COMPLETED")
                .ToList();

            return new StaffPerformanceViewModel
            {
                CompletedToday = completedTasks.Count(t => t.TaskDate.Date == today),
                CompletedThisMonth = completedTasks.Count(t => t.TaskDate.Year == today.Year && t.TaskDate.Month == today.Month),
                TotalCompleted = completedTasks.Count,
                RecentCompletedTasks = completedTasks.OrderByDescending(t => t.TaskDate).Take(10).ToList()
            };
        }
    }
}