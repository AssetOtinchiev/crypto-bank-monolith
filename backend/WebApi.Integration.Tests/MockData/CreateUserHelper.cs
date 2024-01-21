using Microsoft.Extensions.DependencyInjection;
using WebApi.Features.Auth.Services;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Requests;
using WebApi.Features.Users.Services;

namespace WebApi.Integration.Tests.Features.Users.MockData;

public static class CreateUserHelper
{
    public static User CreateUser(string email, RoleType role)
    {
        var existingUser = new User
        {
            Email = email,
            Password = "123",
            RegisteredAt = DateTime.UtcNow,
            DateOfBirth = new DateTime(2000, 01, 31).ToUniversalTime(),
            Roles = new List<Role>
            {
                new()
                {
                    Name = role,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
    }

    public static async Task<User> CreateUser(RegisterUser.Request request, AsyncServiceScope scope, CancellationToken cancellationToken, RoleType? role = RoleType.User)
    {
        var userRegistrationService = scope.ServiceProvider.GetRequiredService<UserRegistrationService>();
        var createdUser = await userRegistrationService.Register(
            request, cancellationToken,
            role);

        return createdUser;
    }

    public static async Task FillAuthToken(HttpClient client, AsyncServiceScope scope, User createdUser, CancellationToken cancellationToken)
    {
        var tokenService = scope.ServiceProvider.GetRequiredService<TokenService>();
        var tokens = await tokenService.GenerateTokensAsync(createdUser, "", cancellationToken);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {tokens.accessToken}");
    }
}