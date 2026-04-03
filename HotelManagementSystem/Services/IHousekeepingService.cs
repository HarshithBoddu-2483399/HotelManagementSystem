using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

public interface IHousekeepingService
{
    IEnumerable<HousekeepingTask> GetPendingTasks();
    IEnumerable<HousekeepingTask> GetAllTasks();
    IEnumerable<HousekeepingTask> GetStaffTasks(int staffId);
    void MarkClean(int taskId);
    IEnumerable<HousekeepingTask> GetCompletedStaffTasks(int staffId);
    StaffPerformanceViewModel GetStaffPerformance(int staffId);
}