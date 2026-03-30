using System.Collections.Generic;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public interface IReservationService
    {
        bool CreateReservation(Reservation reservation, Guest guest);
        void CancelReservation(int reservationId);
        IEnumerable<Reservation> GetAllReservations();
    }
}