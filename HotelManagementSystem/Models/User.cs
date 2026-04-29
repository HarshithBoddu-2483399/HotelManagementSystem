using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        [Required]
        public string? Role { get; set; }

        public string? Name { get; set; }
        public string? Phone { get; set; }
        public bool RequiresPasswordReset { get; set; } = false;
    }
}