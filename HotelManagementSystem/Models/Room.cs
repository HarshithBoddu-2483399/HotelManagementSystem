using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal RatePerNight { get; set; }
        public string Status { get; set; }
    }
}