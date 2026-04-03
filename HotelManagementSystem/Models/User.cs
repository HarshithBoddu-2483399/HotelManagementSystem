using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class User
    {
        [Key] public int UserId { get; set; }
        public string Username { get; set; } 
        public string Password { get; set; } 
        public string Role { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
}