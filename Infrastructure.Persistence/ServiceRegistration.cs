using Application.Interfaces.Infrastructure;
using Domain.Settings;
using Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

       

        services.AddScoped<IAppDbContext>(provider => provider.GetService<AppDbContext>());
    }
    
    public static void AddAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
    {
        //services.Configure<JWTSetting>(configuration.GetSection("JWTSettings"));
        configuration.GetSection(JWTSetting.JWTSectionName).Bind(JWTSetting.JwtOptions);
        //services.AddSingleton(sp=>sp.GetRequiredService<IOptions<JWTSetting>>().Value);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = JWTSetting.JwtOptions.Issuer,
                ValidAudience = JWTSetting.JwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(JWTSetting.JwtOptions.Key)),
                ClockSkew = TimeSpan.Zero
            };
        });
        
        
        services.AddAuthorization(options =>
        {
            // options.AddPolicy(IdentityData.AdminUserPolicyName, p =>
            // {
            //     p.RequireClaim(IdentityData.AdminUserClaimName, "true");
            // });
        });
    }
}