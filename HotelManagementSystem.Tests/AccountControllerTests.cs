using HotelManagementSystem.Controllers;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            _mockAccountService = new Mock<IAccountService>();

            // 1. Mock standard services to prevent "No service for type..." errors
            var authServiceMock = new Mock<IAuthenticationService>();
            var urlHelperMock = new Mock<IUrlHelper>();

            // 2. Setup Service Provider for internal Controller logic
            var serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(authServiceMock.Object);

            // 3. Setup HttpContext
            var httpContextMock = new Mock<HttpContext>();
            httpContextMock.Setup(c => c.RequestServices).Returns(serviceProviderMock.Object);

            // 4. Use real TempDataDictionary to avoid NullRefs
            var tempDataProvider = new Mock<ITempDataProvider>();
            var tempData = new TempDataDictionary(httpContextMock.Object, tempDataProvider.Object);

            // 5. Initialize Controller
            _controller = new AccountController(_mockAccountService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                },
                TempData = tempData,
                Url = urlHelperMock.Object
            };
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Login_Get_ReturnsViewResult()
        {
            var result = _controller.Login();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Login_Post_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            _mockAccountService
                .Setup(s => s.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _controller.Login("baduser", "badpass");

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ViewBag.Error, Is.EqualTo("Invalid credentials"));
        }

        [Test]
        public async Task Login_Post_ValidAdmin_RedirectToReportIndex()
        {
            // Arrange
            var adminUser = new User { UserId = 1, Username = "admin", Role = "Admin" };
            _mockAccountService
                .Setup(s => s.AuthenticateAsync("admin", "pass123"))
                .ReturnsAsync(adminUser);

            // Act
            var result = await _controller.Login("admin", "pass123");

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Report"));
            Assert.That(redirectResult.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public void Register_Get_ReturnsViewResult()
        {
            var result = _controller.Register();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Register_Post_EmailAlreadyExists_ReturnsViewWithError()
        {
            // Arrange
            var model = new GuestRegisterViewModel { Email = "test@test.com" };
            _mockAccountService
                .Setup(s => s.RegisterGuestAsync(It.IsAny<GuestRegisterViewModel>()))
                .ReturnsAsync((false, "Already a user with this email ID exists."));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            Assert.That(_controller.ModelState.IsValid, Is.False);
        }

        [Test]
        public async Task Register_Post_ValidModel_RedirectsToLogin()
        {
            // Arrange
            var model = new GuestRegisterViewModel { Email = "new@test.com" };
            _mockAccountService
                .Setup(s => s.RegisterGuestAsync(It.IsAny<GuestRegisterViewModel>()))
                .ReturnsAsync((true, string.Empty));

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Registration successful! You can now log in."));
        }
    }
}