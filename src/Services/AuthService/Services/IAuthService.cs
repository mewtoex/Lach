using Lach.Shared.Common.Models;

namespace AuthService.Services;

public interface IAuthService
{
    Task<UserLoginResponse> LoginAsync(UserLoginRequest request);
    Task<User> RegisterAsync(UserRegistrationRequest request);
    Task<User?> GetUserByIdAsync(Guid id);
    Task<User?> GetUserByEmailAsync(string email);
    Task<bool> ValidateTokenAsync(string token);
} 