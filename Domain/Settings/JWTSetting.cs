namespace Domain.Settings;

public class JWTSetting
{
    public const string JWTSectionName = "JWTSettings";
    public static JWTOptions JwtOptions { get; set; } = new JWTOptions();
}

public class JWTOptions
{
    public string Key { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public double DurationInMinutes { get; set; }
}