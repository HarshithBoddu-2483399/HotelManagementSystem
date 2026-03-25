using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.Models
{
    public class Guest
    {
        // The [Key] attribute explicitly tells EF Core this is the Primary Key
        [Key]
        public int GuestId { get; set; }

        public string Name { get; set; }
        public string ContactInfo { get; set; }
        public string Email { get; set; }
    }
}