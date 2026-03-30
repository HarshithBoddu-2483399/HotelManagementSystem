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

        public void ToggleMaintenance(int roomId)
        {
            var room = _context.Rooms.Find(roomId);
            if (room != null)
            {
                room.Status = room.Status == "MAINTENANCE" ? "AVAILABLE" : "MAINTENANCE";
                _context.SaveChanges();
            }
        }
    }
}