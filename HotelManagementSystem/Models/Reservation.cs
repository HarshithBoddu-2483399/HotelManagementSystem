using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem.Models
{
    public class Reservation
    {
        [Key] public int ReservationId { get; set; }
        public int GuestId { get; set; }
        public int RoomId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string? ReservationStatus { get; set; }

        // --- Navigation Properties for Eager Loading ---
        [ForeignKey("GuestId")]
        public Guest? Guest { get; set; }

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
    }
}