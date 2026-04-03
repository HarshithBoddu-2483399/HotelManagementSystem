using System;
using System.Collections.Generic;

namespace HotelManagementSystem.ViewModels
{
    public class AttendanceViewModel
    {
        public DateTime SelectedDate { get; set; }
        public List<StaffAttendanceRecord> Records { get; set; } = new();
    }

    public class StaffAttendanceRecord
    {
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public bool? IsPresent { get; set; } // Null if not marked yet
    }
}