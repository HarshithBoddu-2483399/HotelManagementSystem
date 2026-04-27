using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.Controllers;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.Tests.Controllers
{
    [TestFixture]
    public class HousekeepingControllerTests
    {
        private ApplicationDbContext _context;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _context.Rooms.Add(new Room
            {
                RoomId = 1,
                Status = "DIRTY"
            });

            _context.HousekeepingTasks.AddRange(
                new HousekeepingTask
                {
                    TaskId = 1,
                    RoomId = 1,
                    TaskStatus = "ASSIGNED",
                    TaskDate = DateTime.Today
                },
                new HousekeepingTask
                {
                    TaskId = 2,
                    RoomId = 1,
                    TaskStatus = "COMPLETED",
                    TaskDate = DateTime.Today.AddDays(-1),
                    CompletedAt = DateTime.Today.AddDays(-1)
                }
            );

            _context.SaveChanges();
        
        }

        // ✅ Test 1: Index returns only pending tasks
        [Test]
        public void Index_ReturnsOnlyPendingTasks()
        {
            // Arrange
            var controller = new HousekeepingController(_context);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);

            var model = result.Model as List<HousekeepingTask>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(1));
            Assert.That(model.First().TaskStatus, Is.Not.EqualTo("COMPLETED"));
        }

        // ✅ Test 2: AllTasks returns all tasks
        [Test]
        public void AllTasks_ReturnsAllTasks()
        {
            // Arrange
            var controller = new HousekeepingController(_context);

            // Act
            var result = controller.AllTasks() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);

            var model = result.Model as List<HousekeepingTask>;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Count, Is.EqualTo(2));
        }

        // ✅ Test 3: CompleteTask updates task and room correctly
        [Test]
        public void CompleteTask_UpdatesTaskStatusAndRoomStatus()
        {
            // Arrange
            var controller = new HousekeepingController(_context);

            // Act
            var result = controller.CompleteTask(1) as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("StaffIndex"));

            var task = _context.HousekeepingTasks.Find(1);
            var room = _context.Rooms.Find(1);

            Assert.That(task.TaskStatus, Is.EqualTo("COMPLETED"));
            Assert.That(room.Status, Is.EqualTo("AVAILABLE"));
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }
    }
}

