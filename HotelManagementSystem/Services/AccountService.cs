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
            // 1. Check Hardcoded Staff
            if (username == "admin@hotel.com" && password == "Admin@123")
            {
                return new User { Username = "admin@hotel.com", Role = "Admin", UserId = 0 };
            }

            if (username == "manager@hotel.com" && password == "Manager@123")
            {
                return new User { Username = "manager@hotel.com", Role = "Manager", UserId = 0 };
            }

            // 2. Check the Staff / Users table
            var staffUser = _context.Users.FirstOrDefault(u => u.Username == username && u.Password == password);
            if (staffUser != null)
            {
                return staffUser;
            }

            // 3. STRICT GUEST CHECK: Email search, then strict BCrypt verification
            var guestUser = _context.Guests.FirstOrDefault(g => g.Email == username);

            if (guestUser != null && !string.IsNullOrEmpty(guestUser.Password))
            {
                // Only allows login if the hash matches perfectly
                if (BCrypt.Net.BCrypt.Verify(password, guestUser.Password))
                {
                    return new User
                    {
                        UserId = guestUser.GuestId,
                        Username = guestUser.Email,
                        Role = "Guest"
                    };
                }
            }

            return null;
        }
    }
}