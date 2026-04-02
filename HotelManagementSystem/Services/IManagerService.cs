using HotelManagementSystem.ViewModels;
using System.Threading.Tasks;

namespace HotelManagementSystem.Services
{
    public interface IManagerService
    {
        ManagerDashboardViewModel GetManagerDashboardData();
        void AssignStaffToTask(int taskId, int staffId);
    }
}