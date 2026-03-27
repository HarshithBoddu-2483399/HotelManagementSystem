using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly IBillingService _billingService;
        public BillingController(IBillingService billingService) { _billingService = billingService; }

        public IActionResult Index() => View(_billingService.GetInvoices());

        [HttpPost]
        public IActionResult Checkout(int reservationId) => View("Checkout", _billingService.ProcessCheckout(reservationId));
    }
}