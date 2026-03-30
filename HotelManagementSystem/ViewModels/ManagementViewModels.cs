using System;

namespace HotelManagementSystem.ViewModels
{
    // Carries the data for the Invoices table
    public class FinancialRecord
    {
        public int InvoiceId { get; set; }
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Carries the data for the Check-In/Check-Out management table
    public class BookingRecord
    {
        public int ReservationId { get; set; }
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
    }
}