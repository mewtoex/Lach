using AuthService.Services;
using AuthService.Data;
using AuthService.Entities;
using Lach.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Xunit;

namespace AuthService.Tests.Services;

public class AuthServiceTests
{
    private readonly AuthDbContext _context;
    private readonly AuthService.Services.AuthService _authService;
    private readonly JwtService _jwtService;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<AuthDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AuthDbContext(options);
        _jwtService = new JwtService();
        _authService = new AuthService.Services.AuthService(_context, _jwtService);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUser()
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

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.Name.Should().Be(request.Name);
        result.Role.Should().Be(request.Role);

        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        userInDb.Should().NotBeNull();
        userInDb!.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task RegisterAsync_WithDuplicateEmail_ShouldThrowException()
    {
        // Arrange
        var existingUser = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Existing User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(existingUser);
        await _context.SaveChangesAsync();

        var request = new RegisterRequest
        {
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            Password = "password123",
            Role = UserRole.Customer
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.RegisterAsync(request));
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "password123"
        };

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(request.Email);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ShouldThrowException()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "wrongpassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.LoginAsync(request));
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnUser()
    {
        // Arrange
        var user = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        // Act
        var result = await _authService.ValidateTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(user.Email);
        result.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldThrowException()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.ValidateTokenAsync(invalidToken));
    }

    [Fact]
    public async Task GetUserByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new UserEntity
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            Phone = "+5511999999999",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.GetUserByIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(user.Email);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var invalidId = Guid.NewGuid();

        // Act
        var result = await _authService.GetUserByIdAsync(invalidId);

        // Assert
        result.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
} 