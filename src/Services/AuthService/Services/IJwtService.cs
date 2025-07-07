using AuthService.Entities;

namespace AuthService.Services;

public interface IJwtService
{
    string GenerateToken(UserEntity user);
    Guid ValidateToken(string token);
} 