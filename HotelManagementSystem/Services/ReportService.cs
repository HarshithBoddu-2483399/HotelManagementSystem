using System;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        public ReportService(ApplicationDbContext context) { _context = context; }

        public DashboardViewModel GetMetrics()
        {
            return new DashboardViewModel
            {
                TotalRevenue = _context.Invoices.Where(i => i.PaymentStatus == "PAID").Sum(i => i.TotalAmount),
                PendingTasks = _context.HousekeepingTasks.Count(t => t.TaskStatus == "PENDING"),
                RoomsAvailable = _context.Rooms.Count(r => r.Status == "AVAILABLE"),
                RoomsOccupied = _context.Rooms.Count(r => r.Status == "OCCUPIED"),
                RoomsMaintenance = _context.Rooms.Count(r => r.Status == "MAINTENANCE")
            };
        }

        public OccupancyReportViewModel GetOccupancyReport(DateTime? reportDate = null)
        {
            var date = reportDate ?? DateTime.Today;

            // Total rooms in the hotel
            var totalRooms = _context.Rooms.Count();

            // Occupied rooms on this date (reservations checked-in and not yet checked-out on this date)
            var occupiedRooms = _context.Reservations.Count(r =>
                r.ReservationStatus != "CANCELLED" &&
                r.CheckInDate <= date &&
                r.CheckOutDate >= date &&
                (r.ReservationStatus == "CHECKED_IN" || r.ReservationStatus == "CHECKED_OUT" || 
                 r.ReservationStatus == "INVOICED" || r.ReservationStatus == "COMPLETED")
            );

            // Revenue generated on this date (invoices generated on this date that are paid)
            var revenueOnDate = _context.Invoices.Where(i =>
                i.PaymentStatus == "PAID" &&
                i.InvoiceDate.Date == date.Date
            ).Sum(i => i.TotalAmount);

            var occupancyRate = totalRooms > 0 ? (decimal)occupiedRooms / totalRooms * 100 : 0;

            return new OccupancyReportViewModel
            {
                ReportDate = date,
                TotalRooms = totalRooms,
                OccupiedRooms = occupiedRooms,
                OccupancyRate = Math.Round(occupancyRate, 2),
                RevenueGenerated = revenueOnDate
            };
        }

        public RevenueAnalysisViewModel GetRevenueAnalysis(DateTime? startDate = null, DateTime? endDate = null, string dateRange = "custom")
        {
            DateTime start, end;
            var today = DateTime.Today;

            // Calculate date range based on parameter
            switch (dateRange)
            {
                case "last7days":
                    end = today;
                    start = today.AddDays(-7);
                    break;
                case "lastmonth":
                    end = today;
                    start = today.AddMonths(-1);
                    break;
                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1);
                    end = start.AddMonths(1).AddDays(-1);
                    break;
                case "yeartodate":
                    start = new DateTime(today.Year, 1, 1);
                    end = today;
                    break;
                default:
                    start = startDate ?? today.AddMonths(-1);
                    end = endDate ?? today;
                    break;
            }

            // Get paid invoices within the date range
            var invoices = _context.Invoices.Where(i =>
                i.PaymentStatus == "PAID" &&
                i.InvoiceDate.Date >= start.Date &&
                i.InvoiceDate.Date <= end.Date
            ).ToList();

            var totalRevenue = invoices.Sum(i => i.TotalAmount);
            var daysDifference = (end - start).Days + 1;
            var averageRevenuePerDay = daysDifference > 0 ? totalRevenue / daysDifference : 0;

            var dateRangeLabel = dateRange switch
            {
                "last7days" => "Last 7 Days",
                "lastmonth" => "Last Month",
                "thismonth" => "This Month",
                "yeartodate" => "Year to Date",
                _ => $"{start:MMM dd, yyyy} to {end:MMM dd, yyyy}"
            };

            return new RevenueAnalysisViewModel
            {
                TotalRevenue = totalRevenue,
                AverageRevenuePerDay = Math.Round(averageRevenuePerDay, 2),
                InvoiceCount = invoices.Count,
                StartDate = start,
                EndDate = end,
                DateRange = dateRangeLabel
            };
        }
    }
}