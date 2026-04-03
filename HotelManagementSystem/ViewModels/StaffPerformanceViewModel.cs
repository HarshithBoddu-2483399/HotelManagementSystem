using HotelManagementSystem.Models;
using System.Collections.Generic;

namespace HotelManagementSystem.ViewModels
{
    public class StaffPerformanceViewModel
    {
        public int CompletedToday { get; set; }
        public int CompletedThisMonth { get; set; }
        public int TotalCompleted { get; set; }
        public List<HousekeepingTask> RecentCompletedTasks { get; set; } = new();
    }
}