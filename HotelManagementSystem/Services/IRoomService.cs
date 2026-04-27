using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IRoomService
    {
        IEnumerable<Room> GetAllRooms();
        IEnumerable<Room> GetAvailableRooms();
        void AddRoom(Room room);
        Room GetRoomById(int roomId);
        void UpdateRoom(Room updatedRoom);

        (bool Success, string Message) ToggleMaintenance(int roomId);
    }
}