using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Features.Auth.Options;
using WebApi.Features.Auth.Services;
using WebApi.Shared;

namespace WebApi.Features.Auth.Registration;

public static class AuthBuilderExtension
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<ArgonSecurityOptions>(builder.Configuration.GetSection(ArgonSecurityOptions.ArgonSecuritySectionName));
        builder.Services.Configure<JWTSetting>(builder.Configuration.GetSection(JWTSetting.JWTSectionName));

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            var jwtSetting = builder.Configuration.GetSection(JWTSetting.JWTSectionName).Get<JWTSetting>();
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSetting.Issuer,
                ValidAudience = jwtSetting.Audience,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtSetting.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });
        builder.Services.AddAuthorization();
        
        builder.Services.AddTransient<TokenService>();
        builder.Services.AddScoped<PasswordHelper>();
        builder.Services.AddScoped<TokenHelper>();
        
        return builder;
    }
}