using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IHousekeepingService
    {
        IEnumerable<HousekeepingTask> GetPendingTasks();
        IEnumerable<HousekeepingTask> GetAllTasks();
        void MarkClean(int taskId);
    }
}