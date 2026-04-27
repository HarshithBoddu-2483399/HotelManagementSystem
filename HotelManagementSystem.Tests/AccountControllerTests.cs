using HotelManagementSystem.Controllers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace HotelManagementSystem.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests
    {
        private Mock<IAccountService> _mockAccountService;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            // 1. Setup the Mock Service (No more database context needed in the controller tests!)
            _mockAccountService = new Mock<IAccountService>();

            // 2. Setup the tricky HttpContext for SignInAsync/SignOutAsync
            var authServiceMock = new Mock<IAuthenticationService>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // 3. Setup TempData
            var tempDataMock = new Mock<ITempDataDictionary>();

            // 4. Setup the UrlHelper
            var urlHelperMock = new Mock<IUrlHelper>();

            // 5. Initialize Controller with only the Mock Service
            _controller = new AccountController(_mockAccountService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                },
                TempData = tempDataMock.Object,
                Url = urlHelperMock.Object
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up
            _controller?.Dispose();
        }

        [Test]
        public void Login_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Login();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            // UPDATED: Use AuthenticateAsync
            _mockAccountService
                .Setup(s => s.AuthenticateAsync("baduser", "badpass"))
                .ReturnsAsync((User)null);

            // Act
            // UPDATED: Added await
            var result = await _controller.Login("baduser", "badpass");

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task Login_Post_ValidAdmin_RedirectsToReportIndex()
        {
            // Arrange
            var adminUser = new User { UserId = 1, Username = "admin", Role = "Admin" };
            // UPDATED: Use AuthenticateAsync
            _mockAccountService
                .Setup(s => s.AuthenticateAsync("admin", "pass123"))
                .ReturnsAsync(adminUser);

            // Act
            // UPDATED: Added await
            var result = await _controller.Login("admin", "pass123");

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Report"));
        }

        [Test]
        public void Register_Get_ReturnsViewResult()
        {
            // Act
            var result = _controller.Register();

            // Assert
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Register_Post_EmailAlreadyExists_ReturnsViewWithError()
        {
            // Arrange
            var model = new GuestRegisterViewModel
            {
                Email = "test@test.com",
                Phone = "0987654321",
                Password = "Password123!",
                RecoveryPin = "1234"
            };

            // UPDATED: Mock the service to return a failed tuple response
            _mockAccountService
                .Setup(s => s.RegisterGuestAsync(model))
                .ReturnsAsync((false, "Already a user with this email ID exists."));

            // Act
            // UPDATED: Added await
            var result = await _controller.Register(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.ContainsKey("Email"), Is.True);
        }

        [Test]
        public async Task Register_Post_ValidModel_RedirectsToLogin()
        {
            // Arrange
            var model = new GuestRegisterViewModel
            {
                Name = "John Doe",
                Email = "newuser@test.com",
                Phone = "5551234567",
                Password = "Password123!",
                RecoveryPin = "1234"
            };

            // UPDATED: Mock the service to return a successful tuple response
            _mockAccountService
                .Setup(s => s.RegisterGuestAsync(model))
                .ReturnsAsync((true, string.Empty));

            // Act
            // UPDATED: Added await
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Registration successful! You can now log in."));
            // Note: We no longer test database insertions here. That belongs in an 'AccountServiceTests.cs' file!
        }
    }
}