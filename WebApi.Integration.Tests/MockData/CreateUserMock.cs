using WebApi.Features.Users.Domain;

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
}