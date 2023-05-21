using Application.Interfaces;
using Application.Services;
using Application.Validators;
using Contracts.Dtos;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ServiceRegistration
{
    public static void AddApplicationLayer(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserDto>, CreateUserDtoValidator>();
        services.AddScoped<IUserService, UserService>();
        services.AddTransient<ITokenService, TokenService>();
    }
}