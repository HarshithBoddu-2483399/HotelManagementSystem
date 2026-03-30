using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Services;
using System;

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

        [HttpGet]
        public IActionResult OccupancyReport(DateTime? reportDate = null)
        {
            var report = _reportService.GetOccupancyReport(reportDate);
            return View(report);
        }

        [HttpGet]
        public IActionResult RevenueAnalysis(string dateRange = "thismonth", DateTime? startDate = null, DateTime? endDate = null)
        {
            var analysis = _reportService.GetRevenueAnalysis(startDate, endDate, dateRange);
            ViewBag.SelectedDateRange = dateRange;
            return View(analysis);
        }
    }
}