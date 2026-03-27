using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Services
{
    public interface IReportService
    {
        DashboardViewModel GetMetrics();
    }
}