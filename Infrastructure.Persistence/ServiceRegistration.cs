using Application.Interfaces;
using Application.Interfaces.Infrastructure;
using Domain.Settings;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class ServiceRegistration
{
    public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>((provider ,options) =>
        {
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                b =>
                {
                    b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    b.EnableRetryOnFailure(10, TimeSpan.FromSeconds(5), null);
                });
            options.EnableSensitiveDataLogging(true);
        });

        #region Jwt

        services.Configure<JWTSetting>(configuration.GetSection("JWTSettings"));

        #endregion

        services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>());
    }
}