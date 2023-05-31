using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Features.Auth.Options;
using WebApi.Features.Auth.Services;

namespace WebApi.Features.Auth.Registration;

public static class AuthBuilderExtension
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Configuration.GetSection(ArgonSecurityOptions.ArgonSecuritySectionName).Bind(ArgonSecurityOptions.Argon2IdParameters);
        builder.Configuration.GetSection(JWTSetting.JWTSectionName).Bind(JWTSetting.JwtOptions);
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = JWTSetting.JwtOptions.Issuer,
                ValidAudience = JWTSetting.JwtOptions.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(JWTSetting.JwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddAuthorization();
        
        builder.Services.AddTransient<TokenService>();
        return builder;
    }
}