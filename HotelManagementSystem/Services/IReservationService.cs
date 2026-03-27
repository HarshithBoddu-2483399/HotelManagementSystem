using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IReservationService
    {
        bool CreateReservation(Reservation reservation, Guest guest);
    }
}