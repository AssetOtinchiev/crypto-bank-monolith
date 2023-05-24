using WebApi.Database;

namespace WebApi.Features.Users.Registration;

public static class UsersBuilderExtensions
{
    public static WebApplicationBuilder AddUsers(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<AppDbContext>();

        return builder;
    }
}