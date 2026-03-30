using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IGuestService
    {
        IEnumerable<Guest> GetAllGuests();
        Guest GetGuestById(int id);
        void CreateGuest(Guest guest);
    }
}