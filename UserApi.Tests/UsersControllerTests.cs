using Microsoft.AspNetCore.Mvc;
using Moq;
using UserApi.Controllers;
using UserApi.Models;
using UserApi.Services;
using Xunit;

namespace UserApi.Tests;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _serviceMock;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _serviceMock = new Mock<IUserService>();
        _controller = new UsersController(_serviceMock.Object);
    }

    [Fact]
    public void GetAll_ReturnsOk_WithUsers()
    {
        // Arrange
        var users = new List<User> { new() { Id = 1, Name = "Ken", Email = "ken@example.com" } };
        _serviceMock.Setup(s => s.GetAll()).Returns(users);

        // Act
        var result = _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(users, okResult.Value);
    }

    [Fact]
    public void GetById_ReturnsOk_WhenUserExists()
    {
        var user = new User { Id = 1, Name = "Ken", Email = "ken@example.com" };
        _serviceMock.Setup(s => s.GetById(1)).Returns(user);

        var result = _controller.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(user, okResult.Value);
    }

    [Fact]
    public void GetById_ReturnsNotFound_WhenUserDoesNotExist()
    {
        _serviceMock.Setup(s => s.GetById(999)).Returns((User?)null);

        var result = _controller.GetById(999);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetById_Returns500_OnException()
    {
        _serviceMock.Setup(s => s.GetById(It.IsAny<int>())).Throws(new Exception("Boom"));

        var result = _controller.GetById(1);

        var error = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, error.StatusCode);
        Assert.Contains("Internal server error", error.Value?.ToString());
    }

    [Fact]
    public void Create_ReturnsCreated_WhenValid()
    {
        var user = new User { Id = 1, Name = "Ken", Email = "ken@example.com" };
        _controller.ModelState.Clear(); // simulate valid model

        var result = _controller.Create(user);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(user, created.Value);
    }

    [Fact]
    public void Create_ReturnsBadRequest_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Name", "Required");

        var result = _controller.Create(new User());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Update_ReturnsNoContent_WhenUserExists()
    {
        var user = new User { Name = "Updated", Email = "updated@example.com" };
        _serviceMock.Setup(s => s.GetById(1)).Returns(new User());

        var result = _controller.Update(1, user);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Update_ReturnsNotFound_WhenUserMissing()
    {
        _serviceMock.Setup(s => s.GetById(999)).Returns((User?)null);

        var result = _controller.Update(999, new User());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Update_ReturnsBadRequest_WhenModelInvalid()
    {
        _controller.ModelState.AddModelError("Email", "Invalid");

        var result = _controller.Update(1, new User());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public void Delete_ReturnsNoContent_WhenUserExists()
    {
        _serviceMock.Setup(s => s.GetById(1)).Returns(new User());

        var result = _controller.Delete(1);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public void Delete_ReturnsNotFound_WhenUserMissing()
    {
        _serviceMock.Setup(s => s.GetById(999)).Returns((User?)null);

        var result = _controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public void Throw_ThrowsException()
    {
        var ex = Assert.Throws<Exception>(() => _controller.Throw());
        Assert.Equal("Simulated failure", ex.Message);
    }
}