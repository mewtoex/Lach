using AuthService.Controllers;
using AuthService.Services;
using AuthService.Data;
using AuthService.Entities;
using Lach.Shared.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Controllers;

public class AuthControllerTests
{
    private readonly AuthController _controller;
    private readonly Mock<IAuthService> _mockAuthService;

    public AuthControllerTests()
    {
        _mockAuthService = new Mock<IAuthService>();
        _controller = new AuthController(_mockAuthService.Object);
    }

    [Fact]
    public async Task Register_WithValidData_ShouldReturnCreatedResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            Password = "password123",
            Role = UserRole.Customer
        };

        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            Role = request.Role
        };

        _mockAuthService.Setup(x => x.RegisterAsync(request))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result as CreatedAtActionResult;
        createdResult!.Value.Should().Be(expectedUser);
    }

    [Fact]
    public async Task Register_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            Password = "password123",
            Role = UserRole.Customer
        };

        _mockAuthService.Setup(x => x.RegisterAsync(request))
            .ThrowsAsync(new InvalidOperationException("Email already exists"));

        // Act
        var result = await _controller.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnOkResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        var expectedResponse = new LoginResponse
        {
            Token = "valid.jwt.token",
            User = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test User",
                Email = request.Email,
                Role = UserRole.Customer
            }
        };

        _mockAuthService.Setup(x => x.LoginAsync(request))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        _mockAuthService.Setup(x => x.LoginAsync(request))
            .ThrowsAsync(new InvalidOperationException("Invalid credentials"));

        // Act
        var result = await _controller.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ValidateToken_WithValidToken_ShouldReturnOkResult()
    {
        // Arrange
        var token = "valid.jwt.token";
        var expectedUser = new User
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Role = UserRole.Customer
        };

        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedUser);
    }

    [Fact]
    public async Task ValidateToken_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var token = "invalid.token";

        _mockAuthService.Setup(x => x.ValidateTokenAsync(token))
            .ThrowsAsync(new InvalidOperationException("Invalid token"));

        // Act
        var result = await _controller.ValidateToken(token);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetUser_WithValidId_ShouldReturnOkResult()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expectedUser = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Role = UserRole.Customer
        };

        _mockAuthService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be(expectedUser);
    }

    [Fact]
    public async Task GetUser_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _mockAuthService.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetUser(userId);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
} 