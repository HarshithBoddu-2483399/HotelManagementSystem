using HotelManagementSystem.Models;
using System.Collections.Generic;

namespace HotelManagementSystem.ViewModels
{
    public class ManagerDashboardViewModel
    {
        public int TotalRooms { get; set; }
        public int MaintenanceRooms { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int PendingCleaningTasks { get; set; }
        public decimal WeeklyRevenueGrowth { get; set; }

        public List<Invoice> RecentInvoices { get; set; }
        public List<User> AvailableStaff { get; set; }
        public List<HousekeepingTask> PendingTasksList { get; set; }
        public List<Room> AllRooms { get; set; }
    }
}