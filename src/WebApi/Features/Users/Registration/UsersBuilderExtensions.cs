using WebApi.Features.Users.Options;
using WebApi.Features.Users.Services;

namespace WebApi.Features.Users.Registration;

public static class UsersBuilderExtensions
{
    public static WebApplicationBuilder AddUsers(this WebApplicationBuilder builder)
    {
        builder.Services.Configure<UsersOptions>(builder.Configuration.GetSection("Features:Users"));
        builder.Services.AddScoped<UserRegistrationService>();
        return builder;
    }
}