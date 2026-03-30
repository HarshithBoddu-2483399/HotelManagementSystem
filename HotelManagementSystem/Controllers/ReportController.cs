using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }
        public IActionResult Index()
        {
            var dashboardData = _reportService.GetMetrics();
            return View(dashboardData);
        }
    }
}