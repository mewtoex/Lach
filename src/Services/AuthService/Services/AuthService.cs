using AuthService.Data;
using AuthService.Entities;
using Lach.Shared.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class AuthService : IAuthService
{
    private readonly AuthDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(AuthDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request)
    {
        var userEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        if (userEntity == null || !BCrypt.Net.BCrypt.Verify(request.Password, userEntity.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        var token = _jwtService.GenerateToken(userEntity);
        var user = MapToUser(userEntity);

        return new UserLoginResponse
        {
            Token = token,
            User = user,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };
    }

    public async Task<User> RegisterAsync(UserRegistrationRequest request)
    {
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var userEntity = new UserEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = passwordHash,
            Role = request.Role,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        return MapToUser(userEntity);
    }

    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        var userEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

        return userEntity != null ? MapToUser(userEntity) : null;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var userEntity = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        return userEntity != null ? MapToUser(userEntity) : null;
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var userId = _jwtService.ValidateToken(token);
            var user = await GetUserByIdAsync(userId);
            return user != null;
        }
        catch
        {
            return false;
        }
    }

    private static User MapToUser(UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Name = entity.Name,
            Email = entity.Email,
            Phone = entity.Phone,
            Role = entity.Role,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            IsActive = entity.IsActive
        };
    }
} 