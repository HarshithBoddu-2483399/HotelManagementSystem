using HotelManagementSystem.Controllers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace HotelManagementSystem.Tests.Controllers
{
    [TestFixture]
    public class ReservationControllerTests
    {
        private Mock<IReservationService> _mockReservationService;
        private Mock<IRoomService> _mockRoomService;
        private ApplicationDbContext _context;
        private ReservationController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockReservationService = new Mock<IReservationService>();
            _mockRoomService = new Mock<IRoomService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _controller = new ReservationController(
                _mockReservationService.Object,
                _mockRoomService.Object,
                _context
            );
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _context?.Dispose();
        }

        // TEST 1: No RoomId
        [Test]
        public void Create_Get_ReturnsView_WithRooms()
        {
            // Arrange - Must set status to AVAILABLE to show in list
            _context.Rooms.Add(new Room { RoomId = 1, RoomType = "Deluxe", Status = "AVAILABLE" });
            _context.Rooms.Add(new Room { RoomId = 2, RoomType = "Standard", Status = "AVAILABLE" });
            _context.SaveChanges();

            // Act
            var result = _controller.Create(null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            var rooms = _controller.ViewBag.Rooms as List<Room>;
            Assert.That(rooms.Count, Is.EqualTo(2));
        }

        // TEST 2: FIXED - Added Status = "AVAILABLE"
        [Test]
        public void Create_Get_WithRoomId_SetsPreselectedRoomType()
        {
            // Arrange
            var room = new Room { RoomId = 3, RoomType = "Suite", Status = "AVAILABLE" };
            _context.Rooms.Add(room);
            _context.SaveChanges();

            // Act
            var result = _controller.Create(3);

            // Assert
            Assert.That(_controller.ViewBag.SelectedRoomId, Is.EqualTo(3));
            Assert.That(_controller.ViewBag.PreselectedRoomType, Is.EqualTo("Suite"));
        }

        // TEST 3: Room Available
        [Test]
        public void CheckAvailability_WhenRoomAvailable_ReturnsAvailableTrue()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 1, Status = "AVAILABLE" });
            _context.SaveChanges();

            // Act
            var result = _controller.CheckAvailability(1, DateTime.Now, DateTime.Now.AddDays(1)) as JsonResult;

            // Assert
            var property = result.Value.GetType().GetProperty("available");
            bool availableValue = (bool)property.GetValue(result.Value);
            Assert.That(availableValue, Is.True);
        }

        // TEST 4: Overlapping Reservation
        [Test]
        public void CheckAvailability_WhenOverlappingReservation_ReturnsAvailableFalse()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 1, Status = "AVAILABLE" });
            _context.Reservations.Add(new Reservation
            {
                RoomId = 1,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                ReservationStatus = "BOOKED"
            });
            _context.SaveChanges();

            // Act
            var result = _controller.CheckAvailability(1, DateTime.Now, DateTime.Now.AddHours(2)) as JsonResult;

            // Assert
            var property = result.Value.GetType().GetProperty("available");
            bool availableValue = (bool)property.GetValue(result.Value);
            Assert.That(availableValue, Is.False);
        }

        // TEST 5: Index
        [Test]
        public void Index_ReturnsViewWithReservations()
        {
            _mockReservationService.Setup(s => s.GetAllReservations()).Returns(new List<Reservation>());
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // TEST 6: Success
        [Test]
        public void Create_Post_Success_RedirectsToBilling()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 1, Status = "AVAILABLE" });
            _context.SaveChanges();

            var reservation = new Reservation { RoomId = 1 };
            var guest = new Guest();

            _mockReservationService.Setup(s => s.CreateReservation(reservation, guest)).Returns(true);

            // Act
            var result = _controller.Create(reservation, guest);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Billing"));
        }

        // TEST 7: FIXED - Added room to DB so it doesn't fail on null/maintenance check
        [Test]
        public void Create_Post_Failure_ReturnsViewWithError()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 1, Status = "AVAILABLE" });
            _context.SaveChanges();

            var reservation = new Reservation { RoomId = 1 };
            var guest = new Guest();

            // Mock the service to return false (representing a date overlap)
            _mockReservationService.Setup(s => s.CreateReservation(reservation, guest)).Returns(false);

            // Act
            var result = _controller.Create(reservation, guest);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Room unavailable for these specific times."));
        }
    }
}