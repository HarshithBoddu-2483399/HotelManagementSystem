using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Attendance
    {
        [Key] public int AttendanceId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public bool IsPresent { get; set; }
    }
}