using HotelManagementSystem.Controllers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private Mock<ITempDataDictionary> _mockTempData;

        [SetUp]
        public void SetUp()
        {
            _mockRoomService = new Mock<IRoomService>();
            _controller = new RoomController(_mockRoomService.Object);
            _mockTempData = new Mock<ITempDataDictionary>();
            _controller.TempData = _mockTempData.Object;
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Index_ReturnsViewResult_WithAListOfRooms()
        {
            var mockRooms = new List<Room> { new Room { RoomId = 1, RoomNumber = "101" } };
            _mockRoomService.Setup(s => s.GetAllRooms()).Returns(mockRooms);

            var result = _controller.Index() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<List<Room>>());
        }

        [Test]
        public void ToggleMaintenance_Post_Success_RedirectsToIndex()
        {
            // Arrange: Setup the Tuple return value
            _mockRoomService.Setup(s => s.ToggleMaintenance(1))
                            .Returns((true, "Success"));

            // Act
            var result = _controller.ToggleMaintenance(1) as RedirectToActionResult;

            // Assert
            Assert.That(result.ActionName, Is.EqualTo("Index"));
            _mockRoomService.Verify(s => s.ToggleMaintenance(1), Times.Once);
        }

        [Test]
        public void ToggleMaintenance_Post_Failure_SetsTempData()
        {
            // Arrange: Setup a failed Tuple return
            string errorMsg = "Room is occupied";
            _mockRoomService.Setup(s => s.ToggleMaintenance(1))
                            .Returns((false, errorMsg));

            // Act
            var result = _controller.ToggleMaintenance(1) as RedirectToActionResult;

            // Assert
            _mockTempData.VerifySet(t => t["ErrorMessage"] = errorMsg, Times.Once);
            Assert.That(result.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Edit_Post_ValidRoom_RedirectsToIndex()
        {
            var room = new Room { RoomId = 1 };
            _mockRoomService.Setup(s => s.GetRoomById(1)).Returns(room);

            var result = _controller.Edit(room) as RedirectToActionResult;

            Assert.That(result.ActionName, Is.EqualTo("Index"));
            _mockRoomService.Verify(s => s.UpdateRoom(room), Times.Once);
        }
    }
}