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

            // 1. Get the actual start/end dates used by the service
            DateTime start = analysis.StartDate;
            DateTime end = analysis.EndDate;
            int totalDays = (end - start).Days + 1;

            // 2. Logic for Daily vs Weekly grouping
            var chartLabels = new List<string>();
            var chartValues = new List<decimal>();

            if (totalDays <= 8)
            {
                // DAILY GROUPING
                for (var dt = start; dt <= end; dt = dt.AddDays(1))
                {
                    chartLabels.Add(dt.ToString("MMM dd"));
                    chartValues.Add(_reportService.GetRevenueForRange(dt, dt));
                }
            }
            else
            {
                // WEEKLY GROUPING
                for (var dt = start; dt <= end; dt = dt.AddDays(7))
                {
                    var weekEnd = dt.AddDays(6) > end ? end : dt.AddDays(6);
                    chartLabels.Add($"{dt:MMM dd} - {weekEnd:dd}");
                    chartValues.Add(_reportService.GetRevenueForRange(dt, weekEnd));
                }
            }

            ViewBag.ChartLabels = chartLabels;
            ViewBag.ChartValues = chartValues;
            ViewBag.SelectedDateRange = dateRange;

            return View(analysis);
        }
    }
}