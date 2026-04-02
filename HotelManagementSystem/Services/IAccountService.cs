using HotelManagementSystem.Models;
using System.Threading.Tasks;

namespace HotelManagementSystem.Services
{
    public interface IAccountService
    {
        Task<User> Authenticate(string username, string password);
    }
}