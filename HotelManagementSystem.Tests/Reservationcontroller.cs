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
using System.Reflection;

namespace HotelManagementSystem.Tests.Controllers
{
    [TestFixture]
    public class ReservationControllerTests
    {
        private Mock<IReservationService> _mockReservationService;
        private Mock<IRoomService> _mockRoomService;
        private Mock<IGuestService> _mockGuestService;
        private ApplicationDbContext _context;
        private ReservationController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockReservationService = new Mock<IReservationService>();
            _mockRoomService = new Mock<IRoomService>();
            _mockGuestService = new Mock<IGuestService>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _controller = new ReservationController(
                _mockReservationService.Object,
                _mockRoomService.Object,
                _mockGuestService.Object,
                _context
            );
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
            _context?.Dispose();
        }

        // TEST 1:No RoomId
        [Test]
        public void Create_Get_ReturnsView_WithRooms()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 1, RoomType = "Deluxe" });
            _context.Rooms.Add(new Room { RoomId = 2, RoomType = "Standard" });
            _context.SaveChanges();

            // Act
            var result = _controller.Create(null);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ViewBag.Rooms, Is.Not.Null);
        }

        // TEST 2: With RoomId
        [Test]
        public void Create_Get_WithRoomId_SetsPreselectedRoomType()
        {
            // Arrange
            _context.Rooms.Add(new Room { RoomId = 3, RoomType = "Suite" });
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
            // Act
            var result = _controller.CheckAvailability(
                1, DateTime.Now, DateTime.Now.AddDays(1)
            ) as JsonResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            var property = result.Value.GetType().GetProperty("available", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null);

            var availableValue = (bool)property.GetValue(result.Value);
            Assert.That(availableValue, Is.True);
        }

        // TEST 4:Overlapping Reservation
        [Test]
        public void CheckAvailability_WhenOverlappingReservation_ReturnsAvailableFalse()
        {
            // Arrange
            _context.Reservations.Add(new Reservation
            {
                RoomId = 1,
                CheckInDate = DateTime.Now,
                CheckOutDate = DateTime.Now.AddDays(1),
                ReservationStatus = "BOOKED"
            });
            _context.SaveChanges();

            // Act
            var result = _controller.CheckAvailability(
                1, DateTime.Now, DateTime.Now.AddHours(2)
            ) as JsonResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            var property = result.Value.GetType().GetProperty("available", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null);

            var availableValue = (bool)property.GetValue(result.Value);
            Assert.That(availableValue, Is.False);
        }

        // TEST 5: Index
        [Test]
        public void Index_ReturnsViewWithReservations()
        {
            // Arrange
            _mockReservationService
                .Setup(service => service.GetAllReservations())
                .Returns(new List<Reservation>());

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
        }

        // TEST 6:Success
        [Test]
        public void Create_Post_Success_RedirectsToBilling()
        {
            // Arrange
            var reservation = new Reservation { RoomId = 1 };
            var guest = new Guest();

            _mockReservationService
                .Setup(service => service.CreateReservation(reservation, guest))
                .Returns(true);

            // Act
            var result = _controller.Create(reservation, guest);

            // Assert
            Assert.That(reservation.ReservationStatus, Is.EqualTo("BOOKED"));

            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Billing"));
        }

        // TEST 7:Failure
        [Test]
        public void Create_Post_Failure_ReturnsViewWithError()
        {
            // Arrange
            var reservation = new Reservation { RoomId = 1 };
            var guest = new Guest();

            _mockReservationService
                .Setup(service => service.CreateReservation(reservation, guest))
                .Returns(false);

            // Act
            var result = _controller.Create(reservation, guest);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error,
                Is.EqualTo("Room unavailable for these specific times."));
        }
    }
}