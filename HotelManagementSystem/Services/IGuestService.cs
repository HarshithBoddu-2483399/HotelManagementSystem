using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IGuestService
    {
        IEnumerable<Guest> GetAllGuests();
        Guest GetGuestById(int id);

        (bool IsSuccess, string ErrorMessage) CreateGuest(Guest guest);
        IEnumerable<Guest> FindByPhone(string phone);
        Guest FindByEmail(string email);
    }
}