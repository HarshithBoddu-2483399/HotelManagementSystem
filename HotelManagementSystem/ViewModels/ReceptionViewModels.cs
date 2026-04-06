using System;
using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class ReceptionDashboardViewModel
    {
        public int TotalArrivals { get; set; }
        public int TotalDepartures { get; set; }
        public int StayOvers { get; set; }
        public int AvailableRooms { get; set; }
        public List<Room> Rooms { get; set; }
        public Dictionary<int, bool> SafeToBook { get; set; }
    }

    public class RoomDetailsViewModel
    {
        public Room Room { get; set; }
        public Reservation Reservation { get; set; }
        public string GuestName { get; set; }
        public string GuestPhone { get; set; }
        public string GuestEmail { get; set; }
    }

    public class AssignRoomViewModel
    {
        public Room Room { get; set; }
        public List<ReservationDetails> MatchingArrivals { get; set; }
    }

    public class ReservationDetails
    {
        public int ReservationId { get; set; }
        public string GuestName { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}