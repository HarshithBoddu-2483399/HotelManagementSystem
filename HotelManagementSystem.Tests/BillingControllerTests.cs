using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Controllers;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using System;
using System.Linq;

namespace HotelManagementSystem.Tests
{
    [TestFixture]
    public class BillingControllerTests
    {
        // Helper to get a clean in-memory database for every test
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "BillingDb_" + Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private BillingController SetupController(ApplicationDbContext context, Mock<IBillingService> mockBilling, Mock<IReservationService> mockRes)
        {
            return new BillingController(mockBilling.Object, mockRes.Object, context);
        }

        [Test]
        public void CheckOut_SuccessfullyUpdatesRoomToDirty()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var mockBillingService = new Mock<IBillingService>();
            var mockResService = new Mock<IReservationService>();
            var controller = SetupController(context, mockBillingService, mockResService);

            // Seed a room and an active reservation
            var room = new Room { RoomId = 101, Status = "OCCUPIED" };
            var reservation = new Reservation
            {
                ReservationId = 1,
                RoomId = 101,
                Room = room,
                ReservationStatus = "CHECKED-IN",
                CheckOutDate = new DateTime(2026, 4, 15, 11, 0, 0)
            };

            context.Rooms.Add(room);
            context.Reservations.Add(reservation);
            context.SaveChanges();

            // 2. ACT
            var result = controller.CheckOut(reservationId: 1) as RedirectToActionResult;

            // 3. ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Index"));

            // Verify the room status changed
            var updatedRoom = context.Rooms.Find(101);
            Assert.That(updatedRoom.Status, Is.EqualTo("DIRTY"), "Room status must be marked DIRTY after checkout.");

            // Verify the reservation status changed
            var updatedReservation = context.Reservations.Find(1);
            Assert.That(updatedReservation.ReservationStatus, Is.EqualTo("CHECKED-OUT"));
        }

        [Test]
        public void CheckOut_CreatesExactlyOneHousekeepingTask_WithScheduledCheckoutTime()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var mockBillingService = new Mock<IBillingService>();
            var mockResService = new Mock<IReservationService>();
            var controller = SetupController(context, mockBillingService, mockResService);

            var scheduledCheckout = new DateTime(2026, 4, 15, 11, 0, 0);

            var room = new Room { RoomId = 202, Status = "OCCUPIED" };
            var reservation = new Reservation
            {
                ReservationId = 2,
                RoomId = 202,
                Room = room,
                CheckOutDate = scheduledCheckout // The time we expect to be transferred!
            };

            context.Rooms.Add(room);
            context.Reservations.Add(reservation);
            context.SaveChanges();

            // 2. ACT
            controller.CheckOut(reservationId: 2);

            // 3. ASSERT
            var tasks = context.HousekeepingTasks.Where(t => t.RoomId == 202).ToList();

            Assert.That(tasks.Count, Is.EqualTo(1), "Exactly one task should be created.");
            Assert.That(tasks.First().TaskStatus, Is.EqualTo("PENDING"));
            Assert.That(tasks.First().CheckoutTime, Is.EqualTo(scheduledCheckout), "Must use the scheduled checkout time, NOT DateTime.Now");
        }

        [Test]
        public void CheckOut_WhenDuplicateTasksExist_CleansThemUp()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var mockBillingService = new Mock<IBillingService>();
            var mockResService = new Mock<IReservationService>();
            var controller = SetupController(context, mockBillingService, mockResService);

            var room = new Room { RoomId = 303, Status = "OCCUPIED" };
            var reservation = new Reservation { ReservationId = 3, RoomId = 303, Room = room };

            context.Rooms.Add(room);
            context.Reservations.Add(reservation);

            // Deliberately seed THREE pending tasks to simulate the glitch we had earlier
            context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = 303, TaskStatus = "PENDING" });
            context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = 303, TaskStatus = "PENDING" });
            context.HousekeepingTasks.Add(new HousekeepingTask { RoomId = 303, TaskStatus = "PENDING" });

            context.SaveChanges();

            // 2. ACT
            controller.CheckOut(reservationId: 3);

            // 3. ASSERT
            var remainingTasks = context.HousekeepingTasks.Where(t => t.RoomId == 303).ToList();

            // The aggressive cleanup code in the controller should have wiped the extras
            Assert.That(remainingTasks.Count, Is.EqualTo(1), "Duplicate tasks must be deleted, leaving only one.");
        }
    }
}