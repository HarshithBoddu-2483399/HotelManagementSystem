using System.Collections.Generic;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public interface IBillingService
    {
        IEnumerable<FinancialRecord> GetPaidInvoices();
        IEnumerable<FinancialRecord> GetAllInvoices();
        void MarkInvoicePaid(int invoiceId);
        IEnumerable<BookingRecord> GetActiveBookings();
        void CheckInGuest(int reservationId);
        void CheckOutGuest(int reservationId);
        Invoice GenerateInvoice(int reservationId);
        Invoice GetInvoiceByReservation(int reservationId);
    }
}