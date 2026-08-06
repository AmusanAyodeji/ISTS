using Ticketing.Domain.Entities;

namespace Ticketing.Application.Interfaces.Services;

public interface IJwtService
{
    string GenerateToken(User user, IEnumerable<string> roles);
    DateTime GetAccessTokenExpiryUtc();
}
