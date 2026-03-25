using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Data
{
    // 1. Inherit from DbContext so Entity Framework knows this is a database class
    public class ApplicationDbContext : DbContext
    {
        // 2. The Constructor: This passes database configuration (like the connection string) to the base class
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 3. The DbSets: These represent the actual tables in your database

        // Member 1: Inventory & Room Architect
        public DbSet<Room> Rooms { get; set; }

        // Member 2: Booking & Guest Relations Lead
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Reservation> Reservations { get; set; }

        // Member 3: Financial & Checkout Specialist
        public DbSet<Invoice> Invoices { get; set; }

        // Member 4: Operations & Housekeeping Coordinator
        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }

        // Member 5: Analytics & Security Administrator
        public DbSet<OccupancyReport> OccupancyReports { get; set; }
    }
}