using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class GuestService : IGuestService
    {
        private readonly ApplicationDbContext _context;

        public GuestService(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Guest> GetAllGuests() => _context.Guests.ToList();

        public Guest GetGuestById(int id) => _context.Guests.Find(id);

        public IEnumerable<Guest> FindByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return new List<Guest>();
            return _context.Guests.Where(g => g.ContactInfo.StartsWith(phone)).ToList();
        }

        public Guest FindByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return _context.Guests.FirstOrDefault(g => g.Email.ToLower() == email.ToLower());
        }

        public (bool IsSuccess, string ErrorMessage) CreateGuest(Guest guest)
        {
            if (guest == null || string.IsNullOrEmpty(guest.Email))
            {
                return (false, "Please provide guest details.");
            }

            if (string.IsNullOrWhiteSpace(guest.ContactInfo) ||
                guest.ContactInfo.Length != 10 ||
                !guest.ContactInfo.All(char.IsDigit))
            {
                return (false, "Invalid mobile number. Please enter exactly 10 digits.");
            }

            bool duplicateExists = _context.Guests.Any(g => g.ContactInfo == guest.ContactInfo || g.Email == guest.Email);
            if (duplicateExists)
            {
                return (false, "A guest with this phone number or email is already registered.");
            }

            _context.Guests.Add(guest);
            _context.SaveChanges();

            return (true, string.Empty);
        }
    }
}