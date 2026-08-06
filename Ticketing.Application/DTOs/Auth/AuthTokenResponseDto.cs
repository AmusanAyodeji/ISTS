namespace Ticketing.Application.DTOs.Auth;

public class AuthTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiresAt { get; set; }
}
