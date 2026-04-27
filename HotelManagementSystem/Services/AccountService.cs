using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HotelManagementSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public AccountService(ApplicationDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<User> AuthenticateAsync(string username, string password)
        {
            if (username == "admin@hotel.com" && password == "Admin@123")
                return new User { Username = "admin@hotel.com", Role = "Admin", UserId = 0 };

            var staffUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (staffUser != null && !string.IsNullOrEmpty(staffUser.Password))
            {
                if (staffUser.Password == password || _passwordHasher.VerifyPassword(password, staffUser.Password))
                    return staffUser;
            }

            var guestUser = await _context.Guests.FirstOrDefaultAsync(g => g.Email == username);
            if (guestUser != null && !string.IsNullOrEmpty(guestUser.Password))
            {
                if (_passwordHasher.VerifyPassword(password, guestUser.Password))
                {
                    return new User { UserId = guestUser.GuestId, Username = guestUser.Email, Role = "Guest" };
                }
            }

            return null;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> RegisterGuestAsync(GuestRegisterViewModel model)
        {
            if (await CheckEmailExistsAsync(model.Email))
                return (false, "Already a user with this email ID exists.");

            if (await CheckPhoneExistsAsync(model.Phone))
                return (false, "Phone number you are trying has already registered with us. Try resetting password.");

            var newGuest = new Guest
            {
                Name = model.Name,
                Email = model.Email,
                ContactInfo = model.Phone,
                Password = _passwordHasher.HashPassword(model.Password),
                RecoveryPin = _passwordHasher.HashPassword(model.RecoveryPin)
            };

            _context.Guests.Add(newGuest);
            await _context.SaveChangesAsync();
            return (true, string.Empty);
        }

        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _context.Guests.AnyAsync(g => g.Email == email);
        }

        public async Task<bool> CheckPhoneExistsAsync(string phone)
        {
            return await _context.Guests.AnyAsync(g => g.ContactInfo == phone);
        }

        public async Task<Guest> GetGuestByIdAsync(int guestId)
        {
            return await _context.Guests.FindAsync(guestId);
        }

        public async Task<bool> ForceChangePasswordAsync(int guestId, string newPassword, string newRecoveryPin)
        {
            var guest = await _context.Guests.FindAsync(guestId);
            if (guest == null) return false;

            guest.Password = _passwordHasher.HashPassword(newPassword);
            guest.RecoveryPin = _passwordHasher.HashPassword(newRecoveryPin);
            guest.RequiresPasswordReset = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ResetForgotPasswordAsync(GuestForgotPasswordViewModel model)
        {
            var guest = await _context.Guests.FirstOrDefaultAsync(g => g.Email == model.Email && g.ContactInfo == model.Phone);

            if (guest != null && !string.IsNullOrEmpty(guest.RecoveryPin))
            {
                if (_passwordHasher.VerifyPassword(model.RecoveryPin, guest.RecoveryPin))
                {
                    guest.Password = _passwordHasher.HashPassword(model.NewPassword);
                    await _context.SaveChangesAsync();
                    return true;
                }
            }
            return false;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> ClaimAccountAsync(ClaimAccountViewModel model)
        {
            var reservation = await _context.Reservations.FindAsync(model.ReservationId);
            if (reservation == null) return (false, "Verification failed. Reservation not found.");

            var guest = await _context.Guests.FindAsync(reservation.GuestId);
            if (guest != null &&
                guest.Email?.ToLower().Trim() == model.Email.ToLower().Trim() &&
                guest.ContactInfo?.Trim() == model.Phone.Trim())
            {
                guest.Password = _passwordHasher.HashPassword(model.NewPassword);
                guest.RecoveryPin = _passwordHasher.HashPassword(model.RecoveryPin);
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }

            return (false, "Verification failed. Please ensure your Reservation ID, Email, and Phone match your booking exactly.");
        }
    }
}