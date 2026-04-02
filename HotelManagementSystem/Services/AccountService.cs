using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;

        public AccountService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User> Authenticate(string username, string password)
        {
            if (username == "admin@hotel.com" && password == "Admin@123")
            {
                return new User { Username = "admin@hotel.com", Role = "Admin" };
            }

            if (username == "manager@hotel.com" && password == "Manager@123")
            {
                return new User { Username = "manager@hotel.com", Role = "Manager" };
            }

            var user = _context.Users.FirstOrDefault(u =>
                u.Username == username && u.Password == password);

            return user;
        }
    }
}