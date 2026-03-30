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
            // Paid invoices for relevant booking statuses (include invoiced/checked-out/completed)
            var query = from inv in _context.Invoices
                        join res in _context.Reservations on inv.ReservationId equals res.ReservationId
                        join guest in _context.Guests on res.GuestId equals guest.GuestId
                        join room in _context.Rooms on res.RoomId equals room.RoomId
                        where inv.PaymentStatus == "PAID" && (
                            res.ReservationStatus == "BOOKED" ||
                            res.ReservationStatus == "CHECKED_IN" ||
                            res.ReservationStatus == "CHECKED_OUT" ||
                            res.ReservationStatus == "INVOICED" ||
                            res.ReservationStatus == "COMPLETED"
                        )
                        select new FinancialRecord
                        {
                            InvoiceId = inv.InvoiceId,
                            ReservationId = res.ReservationId,
                            GuestName = guest.Name,
                            RoomNumber = room.RoomNumber,
                            InvoiceDate = inv.InvoiceDate,
                            TotalAmount = inv.TotalAmount
                        };
            return query.OrderByDescending(i => i.InvoiceDate).ToList();
        }

        public IEnumerable<FinancialRecord> GetAllInvoices()
        {
            // All paid invoices for any booking status
            var query = from inv in _context.Invoices
                        join res in _context.Reservations on inv.ReservationId equals res.ReservationId
                        join guest in _context.Guests on res.GuestId equals guest.GuestId
                        join room in _context.Rooms on res.RoomId equals room.RoomId
                        where inv.PaymentStatus == "PAID"
                        select new FinancialRecord
                        {
                            InvoiceId = inv.InvoiceId,
                            ReservationId = res.ReservationId,
                            GuestName = guest.Name,
                            RoomNumber = room.RoomNumber,
                            InvoiceDate = inv.InvoiceDate,
                            TotalAmount = inv.TotalAmount
                        };
            return query.OrderByDescending(i => i.InvoiceDate).ToList();
        }

        public void MarkInvoicePaid(int invoiceId)
        {
            var inv = _context.Invoices.Find(invoiceId);
            if (inv != null)
            {
                inv.PaymentStatus = "PAID";
                // Update the reservation status to COMPLETED when invoice is marked as paid
                var res = _context.Reservations.Find(inv.ReservationId);
                if (res != null && res.ReservationStatus == "INVOICED")
                {
                    res.ReservationStatus = "COMPLETED";
                }
                _context.SaveChanges();
            }
        }

        public IEnumerable<BookingRecord> GetActiveBookings()
        {
            // Active / upcoming bookings should include BOOKED, CHECKED_IN, CHECKED_OUT, and INVOICED
            // Exclude COMPLETED status - once paid, reservations move to All Invoices
            var today = DateTime.Today;
            var query = from res in _context.Reservations
                        join guest in _context.Guests on res.GuestId equals guest.GuestId
                        join room in _context.Rooms on res.RoomId equals room.RoomId
                        // include bookings that are:
                        // - BOOKED (future/upcoming), or
                        // - in-progress/invoiced (CHECKED_IN, CHECKED_OUT, INVOICED with recent check-out date)
                        // Exclude COMPLETED - those are finished and appear only in All Invoices
                        where (
                            // Always include newly BOOKED reservations so staff can Check-In on arrival
                            (res.ReservationStatus == "BOOKED")
                            || ((res.ReservationStatus == "CHECKED_IN" || res.ReservationStatus == "CHECKED_OUT" || res.ReservationStatus == "INVOICED") && res.CheckOutDate >= today)
                        )
                        select new BookingRecord
                        {
                            ReservationId = res.ReservationId,
                            GuestId = guest.GuestId,
                            GuestName = guest.Name,
                            RoomNumber = room.RoomNumber,
                            CheckIn = res.CheckInDate,
                            CheckOut = res.CheckOutDate,
                            Status = res.ReservationStatus
                        };
            var list = query.OrderBy(b => b.CheckIn).ToList();
            // Attach invoice id if present
            foreach (var b in list)
            {
                var inv = _context.Invoices.FirstOrDefault(i => i.ReservationId == b.ReservationId);
                if (inv != null) b.InvoiceId = inv.InvoiceId;
                if (inv != null) b.InvoicePaid = inv.PaymentStatus == "PAID";
            }
            return list;
        }

        public void CheckInGuest(int reservationId)
        {
            var res = _context.Reservations.Find(reservationId);
            if (res != null && res.ReservationStatus == "BOOKED")
            {
                res.ReservationStatus = "CHECKED_IN";
                var room = _context.Rooms.Find(res.RoomId);
                if (room != null) room.Status = "OCCUPIED";
                _context.SaveChanges();
            }
        }

        public void CheckOutGuest(int reservationId)
        {
            var res = _context.Reservations.Find(reservationId);
            if (res != null && res.ReservationStatus == "CHECKED_IN")
            {
                res.ReservationStatus = "CHECKED_OUT";
                _context.SaveChanges();
            }
        }

        public Invoice GenerateInvoice(int resId)
        {
            var res = _context.Reservations.Find(resId);
            var room = _context.Rooms.Find(res.RoomId);

            if (_context.Invoices.Any(i => i.ReservationId == resId))
                return _context.Invoices.First(i => i.ReservationId == resId);

            int days = (int)(res.CheckOutDate - res.CheckInDate).TotalDays;
            if (days <= 0) days = 1;

            var inv = new Invoice
            {
                ReservationId = resId,
                InvoiceDate = DateTime.Now,
                TotalAmount = days * room.RatePerNight,
                PaymentStatus = "PENDING"
            };

            _context.Invoices.Add(inv);
            res.ReservationStatus = "INVOICED";

            _context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = room.RoomId, TaskDate = DateTime.Now, TaskStatus = "PENDING" });

            _context.SaveChanges();
            return inv;
        }

        public Invoice GetInvoiceByReservation(int reservationId)
        {
            return _context.Invoices.FirstOrDefault(i => i.ReservationId == reservationId);
        }
    }
}