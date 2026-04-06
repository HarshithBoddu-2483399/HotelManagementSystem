using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using HotelManagementSystem.Data;
using System.Linq;

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
                var reservation = _context.Reservations.Find(reservationId);
                if (reservation != null)
                {
                    reservation.ReservationStatus = "CHECKED-IN";
                    var room = _context.Rooms.Find(reservation.RoomId);
                    if (room != null && room.Status == "AVAILABLE")
                    {
                        room.Status = "OCCUPIED";
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
                var reservation = _context.Reservations.Find(reservationId);
                if (reservation != null)
                {
                    reservation.ReservationStatus = "CHECKED-OUT";
                    var room = _context.Rooms.Find(reservation.RoomId);
                    if (room != null)
                    {
                        room.Status = "DIRTY"; 
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
                    var reservation = _context.Reservations.Find(invoice.ReservationId);
                    if (reservation != null)
                    {
                        reservation.ReservationStatus = "CHECKED-OUT";
                        var room = _context.Rooms.Find(reservation.RoomId);
                        if (room != null)
                        {
                            room.Status = "DIRTY"; 
                        }
                        _context.SaveChanges();
                    }
                }
            }
            catch { /* Failsafe to ensure the page still loads even if DB update fails */ }

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