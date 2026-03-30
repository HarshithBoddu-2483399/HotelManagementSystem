using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IReservationService _resService;

        public BillingController(IBillingService billingService, IReservationService resService)
        {
            _billingService = billingService;
            _resService = resService;
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
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult CheckOut(int reservationId)
        {
            _billingService.CheckOutGuest(reservationId);
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