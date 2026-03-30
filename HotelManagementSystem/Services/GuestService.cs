using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class GuestService : IGuestService
    {
        private readonly ApplicationDbContext _context;
        public GuestService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<Guest> GetAllGuests() => _context.Guests.ToList();

        public Guest GetGuestById(int id) => _context.Guests.Find(id);

        public void CreateGuest(Guest guest)
        {
            _context.Guests.Add(guest);
            _context.SaveChanges();
        }
    }
}