using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UserAccounts.Api.Controllers;
using UserAccounts.Domain.Models;
using UserAccounts.Service.Models;
using UserAccounts.Service.Services;


namespace UserAccounts.UnitTests.Controller
{



    [TestClass]
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;

        public Mock<ILogger<UsersController>> _loggerMock { get; }
        public UsersController _userController { get; }

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _loggerMock = new Mock<ILogger<UsersController>>();
            _userController = new UsersController(_userServiceMock.Object, _loggerMock.Object);
        }

        [TestMethod]
        public async Task CreateUser_ValidUser_ReturnsActionResult()
        {
            // Arrange
            var newUser = new UserModel { FirstName = "John", LastName = "Doe", Email = "t@t.com", DateOfBirth = DateTime.Today.AddYears(-30) };

            // Mocking the repository to return null, indicating that a user with the same name doesn't exist
            _userServiceMock.Setup(r => r.GetUsersAsync()).ReturnsAsync(new List<UserModel>());

            // Act
            var result = await _userController.Create(newUser);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ActionResult));
        }

        [TestMethod]
        public async Task CreateUser_UserWithSameNameExists_ReturnsBadRequest()
        {
            // Arrange
            var existingUser = new UserModel { FirstName = "Jn", LastName = "D", Email = "t@t.com", DateOfBirth = DateTime.Today.AddYears(-35) };
            var newUser = new UserModel { FirstName = "John", LastName = "Doe", Email = "t@t.com", DateOfBirth = DateTime.Today.AddYears(-30) };
            var serviceResponse = new ServiceResponse
            {
                Errors = new List<string> { ValidationMessages.EMAIL_EXISTS },
                Success = false
            };

            // Mocking the repository to return null, indicating that a user with the same name doesn't exist
            _userServiceMock.Setup(r => r.InsertUserAsync(newUser))
                .ReturnsAsync(serviceResponse);

            // Act
            var result = await _userController.Create(newUser) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            Assert.AreEqual(400, result.StatusCode);
            Assert.AreEqual(ValidationMessages.EMAIL_EXISTS, result.Value);
        }

        //More tests to come
    }
}
