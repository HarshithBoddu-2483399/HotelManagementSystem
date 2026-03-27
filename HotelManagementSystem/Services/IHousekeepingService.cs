using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IHousekeepingService
    {
        IEnumerable<HousekeepingTask> GetPendingTasks();
        void MarkClean(int taskId);
    }
}