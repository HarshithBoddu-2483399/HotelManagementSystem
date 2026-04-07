using System;
using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class GuestDashboardViewModel
    {
        public Guest Guest { get; set; }
        public List<GuestBookingInfo> ActiveBookings { get; set; }
        public List<GuestBookingInfo> PastBookings { get; set; }
    }

    public class GuestBookingInfo
    {
        public int ReservationId { get; set; }
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsPaid { get; set; }
        public int? InvoiceId { get; set; }
    }
}