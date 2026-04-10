using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Models;
using System;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager,Receptionist")]
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IReservationService _resService;
        private readonly ApplicationDbContext _context;

        public BillingController(IBillingService billingService, IReservationService resService, ApplicationDbContext context)
        {
            _billingService = billingService;
            _resService = resService;
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.ActiveBookings = _billingService.GetActiveBookings();
            return View();
        }

        [HttpGet]
        public IActionResult AllInvoices()
        {
            var invoices = _billingService.GetAllInvoices();
            return View("AllInvoices", invoices);
        }

        [HttpPost]
        public IActionResult CheckIn(int reservationId)
        {
            _billingService.CheckInGuest(reservationId);

            try
            {
                var reservation = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);
                if (reservation != null)
                {
                    reservation.ReservationStatus = "CHECKED-IN";
                    if (reservation.Room != null && reservation.Room.Status == "AVAILABLE")
                    {
                        reservation.Room.Status = "OCCUPIED";
                    }
                    _context.SaveChanges();
                }
            }
            catch { /* Failsafe */ }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CheckOut(int reservationId)
        {
            _billingService.CheckOutGuest(reservationId);

            try
            {
                var reservation = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);
                if (reservation != null)
                {
                    reservation.ReservationStatus = "CHECKED-OUT";
                    if (reservation.Room != null)
                    {
                        reservation.Room.Status = "DIRTY";

                        // ---> FIX 1: Aggressively grab all pending tasks for this room to prevent duplicates
                        var pendingTasks = _context.HousekeepingTasks
                            .Where(t => t.RoomId == reservation.RoomId && t.TaskStatus == "PENDING")
                            .ToList();

                        HousekeepingTask taskToKeep;

                        if (pendingTasks.Any())
                        {
                            // Keep the first one found
                            taskToKeep = pendingTasks.First();

                            // If there are duplicates, delete them immediately
                            if (pendingTasks.Count > 1)
                            {
                                var duplicateTasks = pendingTasks.Skip(1).ToList();
                                _context.HousekeepingTasks.RemoveRange(duplicateTasks);
                            }
                        }
                        else
                        {
                            // Create a new one if absolutely none exist
                            taskToKeep = new HousekeepingTask
                            {
                                RoomId = reservation.RoomId,
                                TaskStatus = "PENDING"
                            };
                            _context.HousekeepingTasks.Add(taskToKeep);
                        }

                        // ---> FIX 2: Use the SCHEDULED CheckOutDate from the booking, NOT DateTime.Now
                        taskToKeep.CheckoutTime = reservation.CheckOutDate;
                    }
                    _context.SaveChanges();
                }
            }
            catch { /* Failsafe */ }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult GenerateInvoice(int reservationId)
        {
            var inv = _billingService.GenerateInvoice(reservationId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ViewInvoice(int reservationId)
        {
            var invoice = _billingService.GetInvoiceByReservation(reservationId);
            if (invoice == null)
            {
                return NotFound("Invoice not found.");
            }

            var res = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (res != null)
            {
                ViewBag.Reservation = res;
                ViewBag.Room = res.Room;
                ViewBag.Guest = res.Guest;
            }

            return View("Receipt", invoice);
        }

        [HttpPost]
        public IActionResult MarkPaid(int invoiceId)
        {
            _billingService.MarkInvoicePaid(invoiceId);

            try
            {
                var invoice = _context.Invoices.Find(invoiceId);
                if (invoice != null)
                {
                    var reservation = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == invoice.ReservationId);
                    if (reservation != null)
                    {
                        reservation.ReservationStatus = "CHECKED-OUT";
                        if (reservation.Room != null)
                        {
                            reservation.Room.Status = "DIRTY";

                            // ---> FIX 1: Aggressively grab all pending tasks for this room to prevent duplicates
                            var pendingTasks = _context.HousekeepingTasks
                                .Where(t => t.RoomId == reservation.RoomId && t.TaskStatus == "PENDING")
                                .ToList();

                            HousekeepingTask taskToKeep;

                            if (pendingTasks.Any())
                            {
                                // Keep the first one found
                                taskToKeep = pendingTasks.First();

                                // If there are duplicates, delete them immediately
                                if (pendingTasks.Count > 1)
                                {
                                    var duplicateTasks = pendingTasks.Skip(1).ToList();
                                    _context.HousekeepingTasks.RemoveRange(duplicateTasks);
                                }
                            }
                            else
                            {
                                // Create a new one if absolutely none exist
                                taskToKeep = new HousekeepingTask
                                {
                                    RoomId = reservation.RoomId,
                                    TaskStatus = "PENDING"
                                };
                                _context.HousekeepingTasks.Add(taskToKeep);
                            }

                            // ---> FIX 2: Use the SCHEDULED CheckOutDate from the booking, NOT DateTime.Now
                            taskToKeep.CheckoutTime = reservation.CheckOutDate;
                        }
                        _context.SaveChanges();
                    }
                }
            }
            catch { /* Failsafe */ }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(int reservationId)
        {
            _resService.CancelReservation(reservationId);
            return RedirectToAction("Index");
        }
    }
}