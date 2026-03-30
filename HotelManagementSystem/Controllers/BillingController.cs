using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        private readonly IReservationService _resService; // Added to handle cancellations

        public BillingController(IBillingService billingService, IReservationService resService)
        {
            _billingService = billingService;
            _resService = resService;
        }

        // The Ultimate Management View
        public IActionResult Index()
        {
            ViewBag.ActiveBookings = _billingService.GetActiveBookings();
            var invoices = _billingService.GetPaidInvoices();
            return View(invoices);
        }

        [HttpPost]
        public IActionResult CheckIn(int reservationId)
        {
            _billingService.CheckInGuest(reservationId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Cancel(int reservationId)
        {
            _resService.CancelReservation(reservationId);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Checkout(int reservationId)
        {
            var invoice = _billingService.ProcessCheckout(reservationId);
            if (invoice == null) return RedirectToAction("Index"); // Already billed
            return View("Receipt", invoice);
        }
    }
}