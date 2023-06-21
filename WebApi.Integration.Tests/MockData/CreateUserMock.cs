using Microsoft.Extensions.DependencyInjection;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Requests;
using WebApi.Features.Users.Services;

namespace WebApi.Integration.Tests.Features.Users.MockData;

public static class CreateUserMock
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
}