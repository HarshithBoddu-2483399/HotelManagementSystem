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
    }
}