using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WebApi.Features.Auth.Options;
using WebApi.Features.Users.Domain;

namespace WebApi.Features.Auth.Services;

public class TokenHelper
{
    private readonly JWTSetting _jwtSetting;

    public TokenHelper(IOptions<JWTSetting> jwtSetting)
    {
        _jwtSetting = jwtSetting.Value;
    }

    public async Task<string> GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_jwtSetting.Key);

        var roleClaims = new List<Claim>();

        foreach (var userRole in user.Roles)
        {
            var role = userRole.Name;
            roleClaims.Add(new Claim("roles", role.ToString()));
        }

        var claimsIdentity = new ClaimsIdentity(new[]
            {
                new Claim("userid", user.Id.ToString()),
            }.Union(roleClaims)
        );
        
        var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claimsIdentity,
            Issuer = _jwtSetting.Issuer,
            Audience = _jwtSetting.Audience,
            Expires = DateTime.Now.AddMinutes(_jwtSetting.Duration.Minutes),
            SigningCredentials = signingCredentials,
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
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