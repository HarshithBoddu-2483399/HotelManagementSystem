using System;
using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

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
            return GetPaidInvoices();
        }

        public void MarkInvoicePaid(int invoiceId)
        {
            var inv = _context.Invoices.Find(invoiceId);
            if (inv != null)
            {
                inv.PaymentStatus = "PAID";
                var res = _context.Reservations.Find(inv.ReservationId);
                if (res != null)
                {
                    res.ReservationStatus = "COMPLETED";
                }
                _context.SaveChanges();
            }
        }

        public IEnumerable<BookingRecord> GetActiveBookings()
        {
            var reservations = _context.Reservations
                .Where(r => r.ReservationStatus != "COMPLETED" && r.ReservationStatus != "CANCELLED")
                .ToList();

            var result = new List<BookingRecord>();

            foreach (var res in reservations)
            {
                var guest = _context.Guests.Find(res.GuestId);
                var room = _context.Rooms.Find(res.RoomId);
                var inv = _context.Invoices.FirstOrDefault(i => i.ReservationId == res.ReservationId);

                if (inv != null && inv.PaymentStatus == "PAID") continue;

                result.Add(new BookingRecord
                {
                    ReservationId = res.ReservationId,
                    GuestId = res.GuestId,
                    GuestName = guest?.Name ?? "Unknown Guest",
                    RoomNumber = room?.RoomNumber ?? "N/A",
                    CheckIn = res.CheckInDate,
                    CheckOut = res.CheckOutDate,
                    Status = res.ReservationStatus,
                    InvoiceId = inv?.InvoiceId,
                    InvoicePaid = inv?.PaymentStatus == "PAID"
                });
            }

            return result.OrderBy(b => b.CheckIn).ToList();
        }

        public void CheckInGuest(int reservationId)
        {
            var res = _context.Reservations.Find(reservationId);
            if (res != null)
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
            if (res != null)
            {
                res.ReservationStatus = "CHECKED_OUT";
                var room = _context.Rooms.Find(res.RoomId);
                if (room != null) room.Status = "DIRTY";
                _context.SaveChanges();
            }
        }

        public Invoice GenerateInvoice(int resId)
        {
            var res = _context.Reservations.Find(resId);
            var room = _context.Rooms.Find(res.RoomId);

            var existingInv = _context.Invoices.FirstOrDefault(i => i.ReservationId == resId);
            if (existingInv != null) return existingInv;

            double diffHours = (res.CheckOutDate - res.CheckInDate).TotalHours;
            decimal daysToCharge = 0m;

            if (diffHours <= 24)
            {
                daysToCharge = 1.0m; 
            }
            else
            {
                int fullDays = (int)Math.Floor(diffHours / 24.0);
                double extraHours = diffHours % 24.0;

                daysToCharge = fullDays;

                if (extraHours > 0 && extraHours <= 6)
                {
                    daysToCharge += 0.5m;
                }
                else if (extraHours > 6)
                {
                    daysToCharge += 1.0m; 
                }
            }

            var totalAmt = daysToCharge * (room?.RatePerNight ?? 0m);

            var inv = new Invoice
            {
                ReservationId = resId,
                InvoiceDate = DateTime.Now,
                TotalAmount = totalAmt,
                PaymentStatus = "PENDING"
            };

            _context.Invoices.Add(inv);
            res.ReservationStatus = "INVOICED";

            _context.HousekeepingTasks.Add(new HousekeepingTask
            {
                RoomId = res.RoomId,
                TaskDate = DateTime.Now,
                TaskStatus = "PENDING"
            });

            _context.SaveChanges();
            return inv;
        }

        public Invoice GetInvoiceByReservation(int reservationId)
        {
            return _context.Invoices.FirstOrDefault(i => i.ReservationId == reservationId);
        }
    }
}