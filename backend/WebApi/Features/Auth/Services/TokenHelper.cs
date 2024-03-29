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
    private readonly JwtOptions _jwtOptions;

    public TokenHelper(IOptions<AuthOptions> authOptions)
    {
        _jwtOptions = authOptions.Value.Jwt;
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Convert.FromBase64String(_jwtOptions.Key);

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
            Issuer = _jwtOptions.Issuer,
            Audience = _jwtOptions.Audience,
            Expires = DateTime.Now.AddMinutes(_jwtOptions.Duration.Minutes),
            SigningCredentials = signingCredentials,
        };
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
    
    public async Task<string> GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}