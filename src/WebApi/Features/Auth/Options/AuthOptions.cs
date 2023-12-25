namespace WebApi.Features.Auth.Options;

public class AuthOptions
{
    public JwtOptions Jwt { get; set; }
    public TimeSpan RefreshTokenExpiration { get; set; }
}