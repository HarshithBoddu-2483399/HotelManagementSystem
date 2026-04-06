using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class HousekeepingTask
    {
        [Key]
        public int TaskId { get; set; }
        public int RoomId { get; set; }

        public int AssignedStaffId { get; set; }
        public string TaskStatus { get; set; }
        public DateTime TaskDate { get; set; }
        public DateTime? CompletedAt { get; set; }

    }
}