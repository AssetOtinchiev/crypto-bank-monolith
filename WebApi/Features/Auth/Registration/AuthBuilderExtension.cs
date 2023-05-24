using WebApi.Features.Auth.Options;
using WebApi.Features.Auth.Services;

namespace WebApi.Features.Auth.Registration;

public static class AuthBuilderExtension
{
    public static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Configuration.GetSection(JWTSetting.JWTSectionName).Bind(JWTSetting.JwtOptions);
        builder.Services.AddTransient<TokenService>();

        return builder;
    }
}