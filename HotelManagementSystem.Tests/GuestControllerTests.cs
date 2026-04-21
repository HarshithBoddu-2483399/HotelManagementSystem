using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Controllers;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using System.Linq;

namespace HotelManagementSystem.Tests
{
    [TestFixture]
    public class GuestControllerTests
    {
        // Helper method to create a clean, isolated fake database for EVERY test
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "HotelDb_" + System.Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Test]
        public void CreateAjax_WithValidData_SavesGuestToDatabase()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var controller = new GuestController(context);

            var newGuest = new Guest
            {
                Name = "Test Guest",
                Email = "test@example.com",
                ContactInfo = "9998887777"
            };

            // 2. ACT
            var result = controller.CreateAjax(newGuest) as JsonResult;

            // 3. ASSERT (Updated for NUnit 4)
            Assert.That(result, Is.Not.Null, "Controller should return a JsonResult");

            var savedGuest = context.Guests.FirstOrDefault(g => g.Email == "test@example.com");

            Assert.That(savedGuest, Is.Not.Null, "Guest should have been saved to the database.");
            Assert.That(savedGuest.Name, Is.EqualTo("Test Guest"));
            Assert.That(savedGuest.RequiresPasswordReset, Is.True, "System should flag guest for password reset.");
            Assert.That(savedGuest.Password, Is.Not.Null, "BCrypt should have hashed a default password.");
        }

        [Test]
        public void CreateAjax_WithDuplicateEmail_DoesNotSaveToDatabase()
        {
            // 1. ARRANGE
            using var context = GetDbContext();

            context.Guests.Add(new Guest
            {
                Name = "Existing Guest",
                Email = "duplicate@example.com",
                ContactInfo = "1112223333"
            });
            context.SaveChanges();

            var controller = new GuestController(context);

            var duplicateAttempt = new Guest
            {
                Name = "Sneaky Guest",
                Email = "duplicate@example.com",
                ContactInfo = "5556667777"
            };

            // 2. ACT
            var result = controller.CreateAjax(duplicateAttempt) as JsonResult;

            // 3. ASSERT (Updated for NUnit 4)
            Assert.That(result, Is.Not.Null);

            var emailCount = context.Guests.Count(g => g.Email == "duplicate@example.com");
            Assert.That(emailCount, Is.EqualTo(1), "Database should still only have 1 user with this email.");
        }

        [Test]
        public void FindByPhone_WithMatchingPhone_ReturnsData()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            context.Guests.Add(new Guest
            {
                Name = "Phone Lookup Guest",
                Email = "phone@example.com",
                ContactInfo = "8278383739"
            });
            context.SaveChanges();

            var controller = new GuestController(context);

            // 2. ACT
            var result = controller.FindByPhone("827838") as JsonResult;

            // 3. ASSERT (Updated for NUnit 4)
            Assert.That(result, Is.Not.Null, "Should return a JSON array of matches.");
        }
    }
}