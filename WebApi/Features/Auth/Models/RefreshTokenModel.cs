namespace WebApi.Features.Auth.Models;

public class RefreshTokenModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
}