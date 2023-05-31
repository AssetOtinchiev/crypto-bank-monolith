namespace WebApi.Features.Auth.Options;

public class JWTSetting
{
    public const string JWTSectionName = "JWTSettings";
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public TimeSpan Duration { get; set; }
}