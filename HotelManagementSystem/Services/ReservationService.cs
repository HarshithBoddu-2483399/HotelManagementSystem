using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        public ReservationService(ApplicationDbContext context) { _context = context; }

        public bool CreateReservation(Reservation res, Guest guest)
        {
            bool isOverlap = _context.Reservations.Any(r => r.RoomId == res.RoomId && r.ReservationStatus != "CANCELLED" && res.CheckInDate < r.CheckOutDate && res.CheckOutDate > r.CheckInDate);
            if (isOverlap) return false;

            var existingGuest = _context.Guests.FirstOrDefault(g => g.Email == guest.Email);
            if (existingGuest == null) { _context.Guests.Add(guest); _context.SaveChanges(); res.GuestId = guest.GuestId; }
            else { res.GuestId = existingGuest.GuestId; }

            res.ReservationStatus = "BOOKED";
            var room = _context.Rooms.Find(res.RoomId);
            if (room != null) room.Status = "OCCUPIED";

            _context.Reservations.Add(res);
            _context.SaveChanges();
            return true;
        }
    }
}