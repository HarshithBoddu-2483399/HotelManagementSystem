using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context) { _context = context; }

        public DashboardViewModel GetMetrics()
        {
            return new DashboardViewModel
            {
                TotalRevenue = _context.Invoices.Where(i => i.PaymentStatus == "PAID").Sum(i => i.TotalAmount),
                PendingTasks = _context.HousekeepingTasks.Count(t => t.TaskStatus == "PENDING"),
                RoomsAvailable = _context.Rooms.Count(r => r.Status == "AVAILABLE"),
                RoomsOccupied = _context.Rooms.Count(r => r.Status == "OCCUPIED"),
                RoomsMaintenance = _context.Rooms.Count(r => r.Status == "MAINTENANCE")
            };
        }
    }
}