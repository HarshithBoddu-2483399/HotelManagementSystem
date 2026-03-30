using System;
using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public class BillingService : IBillingService
    {
        private readonly ApplicationDbContext _context;
        public BillingService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<FinancialRecord> GetPaidInvoices()
        {
            var query = from inv in _context.Invoices
                        join res in _context.Reservations on inv.ReservationId equals res.ReservationId
                        join guest in _context.Guests on res.GuestId equals guest.GuestId
                        join room in _context.Rooms on res.RoomId equals room.RoomId
                        where inv.PaymentStatus == "PAID"
                        select new FinancialRecord
                        {
                            InvoiceId = inv.InvoiceId,
                            GuestName = guest.Name,
                            RoomNumber = room.RoomNumber,
                            InvoiceDate = inv.InvoiceDate,
                            TotalAmount = inv.TotalAmount
                        };
            return query.ToList();
        }

        public IEnumerable<BookingRecord> GetActiveBookings()
        {
            var query = from res in _context.Reservations
                        join guest in _context.Guests on res.GuestId equals guest.GuestId
                        join room in _context.Rooms on res.RoomId equals room.RoomId
                        where res.ReservationStatus != "CANCELLED"
                        select new BookingRecord
                        {
                            ReservationId = res.ReservationId,
                            GuestName = guest.Name,
                            RoomNumber = room.RoomNumber,
                            CheckIn = res.CheckInDate,
                            CheckOut = res.CheckOutDate,
                            Status = res.ReservationStatus
                        };
            return query.OrderBy(b => b.CheckIn).ToList();
        }

        public void CheckInGuest(int reservationId)
        {
            var res = _context.Reservations.Find(reservationId);
            if (res != null && res.ReservationStatus == "BOOKED")
            {
                res.ReservationStatus = "COMPLETED";
                var room = _context.Rooms.Find(res.RoomId);
                if (room != null) room.Status = "OCCUPIED";
                _context.SaveChanges();
            }
        }

        public Invoice ProcessCheckout(int resId)
        {
            var res = _context.Reservations.Find(resId);
            var room = _context.Rooms.Find(res.RoomId);

            if (_context.Invoices.Any(i => i.ReservationId == resId)) return null;

            int days = (int)(res.CheckOutDate - res.CheckInDate).TotalDays;
            if (days <= 0) days = 1;

            var inv = new Invoice
            {
                ReservationId = resId,
                InvoiceDate = DateTime.Now,
                TotalAmount = days * room.RatePerNight,
                PaymentStatus = "PAID"
            };

            _context.Invoices.Add(inv);
            _context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = room.RoomId, TaskDate = DateTime.Now, TaskStatus = "PENDING" });
            _context.SaveChanges();
            return inv;
        }
    }
}