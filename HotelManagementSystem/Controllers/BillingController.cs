using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Required for .Include()

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
                // Eager Load Room to avoid a second database trip
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
                // Eager Load Room
                var reservation = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);
                if (reservation != null)
                {
                    reservation.ReservationStatus = "CHECKED-OUT";
                    if (reservation.Room != null)
                    {
                        reservation.Room.Status = "DIRTY";
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

            // EAGER LOADING: Fetch Reservation, Room, and Guest in ONE trip
            var res = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .FirstOrDefault(r => r.ReservationId == reservationId);

            if (res != null)
            {
                ViewBag.Reservation = res;
                ViewBag.Room = res.Room;   // Pulled from memory, no DB trip!
                ViewBag.Guest = res.Guest; // Pulled from memory, no DB trip!
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