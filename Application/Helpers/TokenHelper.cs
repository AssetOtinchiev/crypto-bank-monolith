using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Domain.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Helpers;

public class TokenHelper
{
    private readonly JWTSetting _jwtSetting;
    public TokenHelper(IOptions<JWTSetting> jwtSettings)
    {
        _jwtSetting = jwtSettings.Value;
    }
    
    public async Task<string> GenerateAccessToken(int userId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_jwtSetting.Key);
        var claimsIdentity = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        });
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Expires = DateTime.Now.AddMinutes(15),
            SigningCredentials = signingCredentials,
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return await Task.Run(() => tokenHandler.WriteToken(securityToken));
    }
    public static async Task<string> GenerateRefreshToken()
    {
        var secureRandomBytes = new byte[32];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        await System.Threading.Tasks.Task.Run(() => randomNumberGenerator.GetBytes(secureRandomBytes));
        var refreshToken = Convert.ToBase64String(secureRandomBytes);
        return refreshToken;
    }
}