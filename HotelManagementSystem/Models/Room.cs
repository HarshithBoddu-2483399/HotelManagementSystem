using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // 1. Add this

namespace HotelManagementSystem.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }

        [Column(TypeName = "decimal(18,2)")] // 2. Add this
        public decimal RatePerNight { get; set; }

        public string Status { get; set; }
    }
}