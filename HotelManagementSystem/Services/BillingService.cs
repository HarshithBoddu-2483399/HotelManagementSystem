using System;
using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;
        public BillingService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<Invoice> GetInvoices() => _context.Invoices.ToList();

        public Invoice ProcessCheckout(int resId)
        {
            var res = _context.Reservations.Find(resId);
            var room = _context.Rooms.Find(res.RoomId);
            res.ReservationStatus = "COMPLETED";

            int days = (int)(res.CheckOutDate - res.CheckInDate).TotalDays;
            if (days <= 0) days = 1;

            var inv = new Invoice { ReservationId = resId, InvoiceDate = DateTime.Now, TotalAmount = days * room.RatePerNight, PaymentStatus = "PAID" };
            _context.Invoices.Add(inv);

            _context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = room.RoomId, TaskDate = DateTime.Now, TaskStatus = "PENDING" });
            _context.SaveChanges();
            return inv;
        }
    }
}