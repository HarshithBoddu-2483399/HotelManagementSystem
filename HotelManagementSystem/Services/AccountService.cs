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
            // 1. Check Hardcoded Default Admin (Keep this so you don't get locked out!)
            if (username == "admin@hotel.com" && password == "Admin@123")
            {
                return new User { Username = "admin@hotel.com", Role = "Admin", UserId = 0 };
            }

            // 2. STRICT STAFF CHECK: Search by Username (Email), then verify BCrypt hash
            var staffUser = _context.Users.FirstOrDefault(u => u.Username == username);

            if (staffUser != null && !string.IsNullOrEmpty(staffUser.Password))
            {
                // If it's the old hardcoded manager, allow it, otherwise check the hash
                if (staffUser.Password == password || BCrypt.Net.BCrypt.Verify(password, staffUser.Password))
                {
                    return staffUser;
                }
            }

            // 3. STRICT GUEST CHECK: Search by Email, then verify BCrypt hash
            var guestUser = _context.Guests.FirstOrDefault(g => g.Email == username);

            if (guestUser != null && !string.IsNullOrEmpty(guestUser.Password))
            {
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