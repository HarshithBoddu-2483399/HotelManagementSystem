using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HotelManagementSystem.Services;
using System;

namespace HotelManagementSystem.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            // If a Manager accidentally hits /Report/Index, redirect them to their portal
            if (User.IsInRole("Manager"))
            {
                return RedirectToAction("Index", "Manager");
            }

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