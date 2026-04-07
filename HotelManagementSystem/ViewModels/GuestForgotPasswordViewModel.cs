using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.ViewModels
{
    public class GuestForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        public string RecoveryPin { get; set; }
        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
        [Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}