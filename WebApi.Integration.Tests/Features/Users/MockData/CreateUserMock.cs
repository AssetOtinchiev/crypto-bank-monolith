using WebApi.Features.Users.Domain;

namespace WebApi.Integration.Tests.Features.Users.MockData;

public static class CreateUserMock
{
    public static User CreateAdminUser(string email)
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
                    Name = RoleType.Administrator,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
    }
    
    public static User CreateUser(string email)
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
                    Name = RoleType.User,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
        return existingUser;
    }
}