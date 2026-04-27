using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System.Threading.Tasks;

namespace HotelManagementSystem.Services
{
    public interface IAccountService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<(bool IsSuccess, string ErrorMessage)> RegisterGuestAsync(GuestRegisterViewModel model);
        Task<bool> CheckEmailExistsAsync(string email);
        Task<bool> CheckPhoneExistsAsync(string phone);
        Task<bool> ForceChangePasswordAsync(int guestId, string newPassword, string newRecoveryPin);
        Task<bool> ResetForgotPasswordAsync(GuestForgotPasswordViewModel model);
        Task<(bool IsSuccess, string ErrorMessage)> ClaimAccountAsync(ClaimAccountViewModel model);
        Task<Guest> GetGuestByIdAsync(int guestId);
    }
}