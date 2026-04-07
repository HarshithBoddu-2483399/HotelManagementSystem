using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Guest
    {
        [Key] public int GuestId { get; set; }
        public string Name { get; set; }
        public string? Email { get; set; }
        public string? ContactInfo { get; set; }
        public string? Password { get; set; }
        public string? RecoveryPin { get; set; }

        public bool RequiresPasswordReset { get; set; }
    }
}