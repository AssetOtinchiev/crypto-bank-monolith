namespace WebApi.Features.Auth.Models;

public class AccessTokenModel
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}