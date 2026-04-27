using System.Collections.Generic;
using System.Linq;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class RoomService : IRoomService
    {
        private readonly ApplicationDbContext _context;
        public RoomService(ApplicationDbContext context) { _context = context; }

        public IEnumerable<Room> GetAllRooms() => _context.Rooms.ToList();
        public IEnumerable<Room> GetAvailableRooms() => _context.Rooms.Where(r => r.Status == "AVAILABLE").ToList();

        public void AddRoom(Room room)
        {
            room.Status = "AVAILABLE";
            _context.Rooms.Add(room);
            _context.SaveChanges();
        }

        public Room GetRoomById(int roomId)
        {
            return _context.Rooms.Find(roomId);
        }

        public void UpdateRoom(Room updatedRoom)
        {
            var existingRoom = _context.Rooms.Find(updatedRoom.RoomId);
            if (existingRoom != null)
            {
                existingRoom.RoomNumber = updatedRoom.RoomNumber;
                existingRoom.RoomType = updatedRoom.RoomType;
                existingRoom.RatePerNight = updatedRoom.RatePerNight;
                _context.SaveChanges();
            }
        }

        // UPDATED: Strict status validation
        public (bool Success, string Message) ToggleMaintenance(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room != null)
            {
                if (room.Status == "AVAILABLE")
                {
                    room.Status = "MAINTENANCE";
                    _context.SaveChanges();
                    return (true, "Room placed under maintenance.");
                }
                else if (room.Status == "MAINTENANCE")
                {
                    room.Status = "AVAILABLE";
                    _context.SaveChanges();
                    return (true, "Room is now available.");
                }
                else
                {
                    // Fails safely for BOOKED, OCCUPIED, DIRTY, etc.
                    return (false, $"Room is currently {room.Status} and cannot be kept under maintenance.");
                }
            }
            return (false, "Room not found.");
        }
    }
}