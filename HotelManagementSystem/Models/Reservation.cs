using System;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Reservation
    {
        [Key]
        public int ReservationId { get; set; }

        // Foreign Keys
        public int GuestId { get; set; }
        public int RoomId { get; set; }

        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string ReservationStatus { get; set; } // BOOKED, CANCELLED, COMPLETED
    }
}