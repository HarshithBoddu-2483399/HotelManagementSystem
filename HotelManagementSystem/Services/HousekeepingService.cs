using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

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
    }
}