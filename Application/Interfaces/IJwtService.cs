using Domain.Models;

namespace Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(ApplicationUser user);
    RefreshToken GenerateRefreshToken(string userId);
    int GetRefreshTokenExpirationDays();
}
