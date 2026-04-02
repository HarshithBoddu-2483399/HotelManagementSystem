using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.Services
{
    public class ManagerService : IManagerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IReportService _reportService;

        public bool IsSuccess { get; private set; }

        public ManagerService(ApplicationDbContext context, IReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public ManagerDashboardViewModel GetManagerDashboardData()
        {
            var metrics = _reportService.GetMetrics();
            return new ManagerDashboardViewModel
            {
                TotalRevenue = metrics.TotalRevenue,
                OccupiedRooms = metrics.RoomsOccupied,
                AvailableRooms = metrics.RoomsAvailable,
                PendingCleaningTasks = metrics.PendingTasks,
                RecentInvoices = _context.Invoices.OrderByDescending(i => i.InvoiceDate).Take(10).ToList(),
                AvailableStaff = _context.Users.Where(u => u.Role == "Housekeeping").ToList()
            };
        }

        public void AssignStaffToTask(int taskId, int staffId)
        {
            var task = _context.HousekeepingTasks.Find(taskId);
            if (task != null)
            {
                task.AssignedStaffId = staffId;
                task.TaskStatus = "ASSIGNED";
                _context.SaveChanges();
            }
        }
    }
}