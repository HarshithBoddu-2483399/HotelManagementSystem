using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore; // Required for .Include()

namespace HotelManagementSystem.Services
{
    public class ReservationService : IReservationService
    {
        private readonly ApplicationDbContext _context;
        public ReservationService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<Reservation> GetAllReservations() =>
            _context.Reservations
                .Include(r => r.Guest)  // Eager Load Guests
                .Include(r => r.Room)   // Eager Load Rooms
                .ToList();

        public bool CreateReservation(Reservation res, Guest guest)
        {
            bool isOverlap = _context.Reservations.Any(r =>
                r.RoomId == res.RoomId &&
                r.ReservationStatus != "CANCELLED" &&
                r.ReservationStatus != "COMPLETED" &&
                res.CheckInDate < r.CheckOutDate.AddHours(1) &&
                res.CheckOutDate > r.CheckInDate);

            if (isOverlap) return false;

            var existingGuest = _context.Guests.FirstOrDefault(g => g.Email == guest.Email);
            if (existingGuest == null)
            {
                _context.Guests.Add(guest);
                _context.SaveChanges();
                res.GuestId = guest.GuestId;
            }
            else { res.GuestId = existingGuest.GuestId; }

            res.ReservationStatus = "BOOKED";

            var room = _context.Rooms.Find(res.RoomId);
            if (room != null && room.Status == "AVAILABLE")
            {
                room.Status = "BOOKED";
            }

            _context.Reservations.Add(res);
            _context.SaveChanges();
            return true;
        }

        public void CancelReservation(int reservationId)
        {
            var res = _context.Reservations.Include(r => r.Room).FirstOrDefault(r => r.ReservationId == reservationId);

            if (res != null && res.ReservationStatus == "BOOKED")
            {
                res.ReservationStatus = "CANCELLED";

                if (res.Room != null && res.Room.Status == "BOOKED")
                {
                    res.Room.Status = "AVAILABLE";
                }

                _context.SaveChanges();
            }
        }
    }
}