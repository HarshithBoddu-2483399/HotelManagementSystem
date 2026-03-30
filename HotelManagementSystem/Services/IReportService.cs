using HotelManagementSystem.ViewModels;
using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Services
{
    public interface IReportService
    {
        DashboardViewModel GetMetrics();
        OccupancyReportViewModel GetOccupancyReport(DateTime? reportDate = null);
        RevenueAnalysisViewModel GetRevenueAnalysis(DateTime? startDate = null, DateTime? endDate = null, string dateRange = "custom");
    }
}