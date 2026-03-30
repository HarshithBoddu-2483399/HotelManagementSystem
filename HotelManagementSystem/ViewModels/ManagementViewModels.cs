using System;

namespace HotelManagementSystem.ViewModels
{
    // Carries the data for the Invoices table
    public class FinancialRecord
    {
        public int InvoiceId { get; set; }
        public int ReservationId { get; set; }
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalAmount { get; set; }
    }

    // Carries the data for the Check-In/Check-Out management table
    public class BookingRecord
    {
        public int ReservationId { get; set; }
        public int GuestId { get; set; }
        public string GuestName { get; set; }
        public string RoomNumber { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public string Status { get; set; }
        public int? InvoiceId { get; set; }
        public bool? InvoicePaid { get; set; }
    }

    // Occupancy Report View Model
    public class OccupancyReportViewModel
    {
        public DateTime ReportDate { get; set; }
        public int TotalRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public decimal OccupancyRate { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    // Revenue Analysis View Model
    public class RevenueAnalysisViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal AverageRevenuePerDay { get; set; }
        public int InvoiceCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string DateRange { get; set; }
    }
}