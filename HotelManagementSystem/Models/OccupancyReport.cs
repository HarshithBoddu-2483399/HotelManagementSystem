using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 1. Add this

namespace HotelManagementSystem.Models
{
    public class OccupancyReport
    {
        [Key]
        public int ReportId { get; set; }
        public DateTime ReportDate { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }

        [Column(TypeName = "decimal(18,2)")] // 2. Add this
        public decimal RevenueGenerated { get; set; }
    }
}