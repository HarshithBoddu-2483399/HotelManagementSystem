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
            // 1. EAGER LOAD Reservations with their Guests and Rooms
            var reservations = _context.Reservations
                .Include(r => r.Guest)
                .Include(r => r.Room)
                .Where(r => r.ReservationStatus != "COMPLETED" && r.ReservationStatus != "CANCELLED")
                .ToList();

            // 2. Fetch relevant Invoices in a single query
            var reservationIds = reservations.Select(r => r.ReservationId).ToList();
            var invoices = _context.Invoices
                .Where(i => reservationIds.Contains(i.ReservationId))
                .ToList();

            var result = new List<BookingRecord>();

            foreach (var res in reservations)
            {
                // Find invoice in memory (no DB trip inside this loop!)
                var inv = invoices.FirstOrDefault(i => i.ReservationId == res.ReservationId);

                if (inv != null && inv.PaymentStatus == "PAID") continue;

                result.Add(new BookingRecord
                {
                    ReservationId = res.ReservationId,
                    GuestId = res.GuestId,
                    GuestName = res.Guest?.Name ?? "Unknown Guest",
                    RoomNumber = res.Room?.RoomNumber ?? "N/A",
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
            var res = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);
            if (res != null)
            {
                res.ReservationStatus = "CHECKED_IN";
                if (res.Room != null) res.Room.Status = "OCCUPIED";
                _context.SaveChanges();
            }
        }

        public void CheckOutGuest(int reservationId)
        {
            var res = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);
            if (res != null)
            {
                res.ReservationStatus = "CHECKED_OUT";
                if (res.Room != null) res.Room.Status = "DIRTY";
                _context.SaveChanges();
            }
        }

        public Invoice GenerateInvoice(int resId)
        {
            // Eager Load Room
            var res = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == resId);
            if (res == null) return null;

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

            var totalAmt = daysToCharge * (res.Room?.RatePerNight ?? 0m);

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