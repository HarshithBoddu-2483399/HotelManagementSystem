using System.Collections.Generic;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public interface IBillingService
    {
        IEnumerable<FinancialRecord> GetPaidInvoices();
        IEnumerable<BookingRecord> GetActiveBookings();
        void CheckInGuest(int reservationId);
        Invoice ProcessCheckout(int reservationId);
    }
}