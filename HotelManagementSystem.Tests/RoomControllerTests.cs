using HotelManagementSystem.Controllers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace HotelManagementSystem.Tests.Controllers
{
    [TestFixture]
    public class RoomControllerTests
    {
        private Mock<IRoomService> _mockRoomService;
        private RoomController _controller;

        [SetUp]
        public void SetUp()
        {
            // Runs before EVERY test.
            _mockRoomService = new Mock<IRoomService>();
            _controller = new RoomController(_mockRoomService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            // Runs after EVERY test to clean up resources and fix the Dispose error.
            _controller?.Dispose();
        }

        [Test]
        public void Index_ReturnsViewResult_WithAListOfRooms()
        {
            // Arrange
            var mockRooms = new List<Room>
            {
                new Room { RoomId = 1, RoomNumber = "101" },
                new Room { RoomId = 2, RoomNumber = "102" }
            };
            _mockRoomService.Setup(service => service.GetAllRooms()).Returns(mockRooms);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null, "Expected a ViewResult");

            var model = viewResult.Model as List<Room>;
            Assert.That(model, Is.Not.Null, "Expected model to be a List<Room>");
            Assert.That(model.Count, Is.EqualTo(2));
        }

        [Test]
        public void Create_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void Create_Post_CallsAddRoomAndRedirectsToIndex()
        {
            // Arrange
            var newRoom = new Room { RoomId = 3, RoomNumber = "103" };

            // Act
            var result = _controller.Create(newRoom);

            // Assert
            _mockRoomService.Verify(service => service.AddRoom(newRoom), Times.Once);

            var redirectToActionResult = result as RedirectToActionResult;
            Assert.That(redirectToActionResult, Is.Not.Null);
            Assert.That(redirectToActionResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Edit_Get_ValidId_ReturnsViewResult_WithRoom()
        {
            // Arrange
            var existingRoom = new Room { RoomId = 1, RoomNumber = "101" };
            _mockRoomService.Setup(service => service.GetRoomById(1)).Returns(existingRoom);

            // Act
            var result = _controller.Edit(1);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.EqualTo(existingRoom));
        }

        [Test]
        public void Edit_Get_InvalidId_ReturnsNotFoundView()
        {
            // Arrange
            _mockRoomService.Setup(service => service.GetRoomById(99)).Returns((Room)null);

            // Act
            var result = _controller.Edit(99);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.ViewName, Is.EqualTo("NotFound"));
        }

        [Test]
        public void Edit_Post_ValidRoom_CallsUpdateAndRedirects()
        {
            // Arrange
            var updatedRoom = new Room { RoomId = 1, RoomNumber = "101 Updated" };
            _mockRoomService.Setup(service => service.GetRoomById(1)).Returns(new Room { RoomId = 1 });

            // Act
            var result = _controller.Edit(updatedRoom);

            // Assert
            _mockRoomService.Verify(service => service.UpdateRoom(updatedRoom), Times.Once);

            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Edit_Post_InvalidRoom_ReturnsNotFoundView()
        {
            // Arrange
            var invalidRoom = new Room { RoomId = 99 };
            _mockRoomService.Setup(service => service.GetRoomById(99)).Returns((Room)null);

            // Act
            var result = _controller.Edit(invalidRoom);

            // Assert
            _mockRoomService.Verify(service => service.UpdateRoom(It.IsAny<Room>()), Times.Never);

            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.ViewName, Is.EqualTo("NotFound"));
        }

        [Test]
        public void ToggleMaintenance_Post_CallsToggleAndRedirects()
        {
            // Act
            var result = _controller.ToggleMaintenance(1);

            // Assert
            _mockRoomService.Verify(service => service.ToggleMaintenance(1), Times.Once);

            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }
    }
}