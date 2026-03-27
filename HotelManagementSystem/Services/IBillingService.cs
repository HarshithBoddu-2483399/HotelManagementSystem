using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IBillingService
    {
        IEnumerable<Invoice> GetInvoices();
        Invoice ProcessCheckout(int reservationId);
    }
}