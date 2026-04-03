using System;

namespace HotelManagementSystem.ViewModels
{
    public class PerformanceReportViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? SelectedStaffId { get; set; }
        public int TotalAssigned { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedDelayed { get; set; }

        public double Accuracy
        {
            get
            {
                if (TotalAssigned == 0) return 0;
                return Math.Round((double)(CompletedOnTime + CompletedDelayed) / TotalAssigned * 100, 2);
            }
        }
    }
}