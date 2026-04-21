using HotelManagementSystem.Controllers;
using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;
using HotelManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
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
        private ApplicationDbContext _context;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            // 1. Setup the In-Memory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
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

            // 4. Setup the UrlHelper to fix the RedirectToAction crash!
            var urlHelperMock = new Mock<IUrlHelper>();

            // 5. Initialize Controller with all our fakes
            _controller = new AccountController(_mockAccountService.Object, _context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = httpContextMock.Object
                },
                TempData = tempDataMock.Object,
                Url = urlHelperMock.Object // <--- ADD THIS LINE
            };
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up to prevent memory leaks
            _context?.Database.EnsureDeleted();
            _context?.Dispose();
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
            // Simulate service returning null for bad credentials
            _mockAccountService
                .Setup(s => s.Authenticate("baduser", "badpass"))
                .ReturnsAsync((User)null); // Replace 'User' with your exact auth user model name

            // Act
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
            _mockAccountService
                .Setup(s => s.Authenticate("admin", "pass123"))
                .ReturnsAsync(adminUser);

            // Act
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
        public void Register_Post_EmailAlreadyExists_ReturnsViewWithError()
        {
            // Arrange
            // Add a guest to our In-Memory database so it "exists"
            _context.Guests.Add(new Guest { Email = "test@test.com", ContactInfo = "1234567890" });
            _context.SaveChanges();

            var model = new GuestRegisterViewModel
            {
                Email = "test@test.com",
                Phone = "0987654321",
                Password = "Password123!",
                RecoveryPin = "1234"
            };

            // Act
            var result = _controller.Register(model);

            // Assert
            var viewResult = result as ViewResult;
            Assert.That(viewResult, Is.Not.Null);
            // Verify the ModelState contains the error we expect
            Assert.That(_controller.ModelState.ContainsKey("Email"), Is.True);
        }

        [Test]
        public void Register_Post_ValidModel_AddsGuestAndRedirectsToLogin()
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

            // Act
            var result = _controller.Register(model);

            // Assert
            // 1. Check the response type
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult, Is.Not.Null);
            Assert.That(redirectResult.ActionName, Is.EqualTo("Login"));

            // 2. Verify TempData success message was set
            Assert.That(_controller.TempData["Success"], Is.EqualTo("Registration successful! You can now log in."));

            // 3. Verify the Guest was actually saved to our in-memory database
            var savedGuest = _context.Guests.FirstOrDefault(g => g.Email == "newuser@test.com");
            Assert.That(savedGuest, Is.Not.Null);
            Assert.That(savedGuest.Name, Is.EqualTo("John Doe"));

            // 4. Verify password was hashed (it shouldn't equal plain text)
            Assert.That(savedGuest.Password, Is.Not.EqualTo("Password123!"));
        }
    }
}