using NUnit.Framework;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Controllers;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace HotelManagementSystem.Tests
{
    [TestFixture]
    public class ManagerControllerTests
    {
        // Helper to get a clean in-memory database
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "ManagerDb_" + Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        // Helper to set up TempData for the controller (since we test TempData messages)
        private ManagerController SetupController(ApplicationDbContext context, IManagerService service)
        {
            // FIX: Created a dummy mock for IRoomService to satisfy the updated constructor
            var mockRoomService = new Mock<IRoomService>();

            // FIX: Injected mockRoomService.Object into the controller instantiation
            var controller = new ManagerController(service, context, mockRoomService.Object)
            {
                TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>())
            };
            return controller;
        }

        [Test]
        public void Housekeeping_ReturnsView_WithPendingTasks_AndAvailableStaff()
        {
            // 1. ARRANGE
            using var context = GetDbContext();

            // Add one PENDING and one COMPLETED task to ensure it only grabs PENDING
            context.HousekeepingTasks.Add(new HousekeepingTask { TaskId = 1, RoomId = 101, TaskStatus = "PENDING" });
            context.HousekeepingTasks.Add(new HousekeepingTask { TaskId = 2, RoomId = 102, TaskStatus = "COMPLETED" });
            context.SaveChanges();

            // Mock the service to return fake staff
            var mockService = new Mock<IManagerService>();
            mockService.Setup(s => s.GetAllStaff()).Returns(new List<User>
            {
                new User { UserId = 1, Name = "John", Role = "Housekeeping" },
                new User { UserId = 2, Name = "Admin", Role = "Admin" } // Should be filtered out
            });

            var controller = SetupController(context, mockService.Object);

            // 2. ACT
            var result = controller.Housekeeping() as ViewResult;

            // 3. ASSERT
            Assert.That(result, Is.Not.Null);

            // Verify ViewBag has only the Housekeeping staff
            var availableStaff = result.ViewData["AvailableStaff"] as List<User>;
            Assert.That(availableStaff, Is.Not.Null);
            Assert.That(availableStaff.Count, Is.EqualTo(1));
            Assert.That(availableStaff.First().Name, Is.EqualTo("John"));

            // Verify the Model only passed the PENDING task
            var model = result.Model as List<HousekeepingTask>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.First().RoomId, Is.EqualTo(101));
        }

        [Test]
        public void AssignStaffToTask_WhenTimeIsPast1HourDeadline_BlocksAssignment()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var mockService = new Mock<IManagerService>();
            var controller = SetupController(context, mockService.Object);

            // A guest checked out at 10:00 AM (Deadline is 11:00 AM)
            var checkoutTime = new DateTime(2026, 4, 15, 10, 0, 0);
            context.HousekeepingTasks.Add(new HousekeepingTask
            {
                TaskId = 1,
                RoomId = 101,
                CheckoutTime = checkoutTime
            });
            context.SaveChanges();

            // Manager attempts to assign the task for 11:30 AM (Too late!)
            var targetDate = new DateTime(2026, 4, 15);
            var badTime = new TimeSpan(11, 30, 0);

            // 2. ACT
            var result = controller.AssignStaffToTask(taskId: 1, staffId: 5, targetDate, deadlineTime: badTime) as RedirectToActionResult;

            // 3. ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Housekeeping"));

            // Verify our strict backend security blocked it
            Assert.That(controller.TempData["ErrorMessage"], Is.Not.Null);
            Assert.That(controller.TempData["ErrorMessage"].ToString(), Does.Contain("Assignment blocked"));

            // Verify the service was NEVER called to save the bad assignment
            mockService.Verify(s => s.AssignStaffToTask(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<TimeSpan>()), Times.Never);
        }

        [Test]
        public void AssignStaffToTask_WhenTimeIsValid_AssignsSuccessfully()
        {
            // 1. ARRANGE
            using var context = GetDbContext();
            var mockService = new Mock<IManagerService>();
            var controller = SetupController(context, mockService.Object);

            // A guest checked out at 10:00 AM (Deadline is 11:00 AM)
            var checkoutTime = new DateTime(2026, 4, 15, 10, 0, 0);
            context.HousekeepingTasks.Add(new HousekeepingTask
            {
                TaskId = 1,
                RoomId = 101,
                CheckoutTime = checkoutTime
            });
            context.SaveChanges();

            // Manager attempts to assign the task for 10:45 AM (Valid!)
            var targetDate = new DateTime(2026, 4, 15);
            var validTime = new TimeSpan(10, 45, 0);

            // 2. ACT
            var result = controller.AssignStaffToTask(taskId: 1, staffId: 5, targetDate, deadlineTime: validTime) as RedirectToActionResult;

            // 3. ASSERT
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Housekeeping"));

            // Verify success message
            Assert.That(controller.TempData["SuccessMessage"], Is.EqualTo("Staff assigned successfully."));

            // Verify the service WAS called perfectly
            mockService.Verify(s => s.AssignStaffToTask(1, 5, targetDate, validTime), Times.Once);
        }
    }
}