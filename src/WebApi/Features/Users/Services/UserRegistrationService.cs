using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Options;
using WebApi.Features.Users.Requests;
using WebApi.Shared;

namespace WebApi.Features.Users.Services;

public class UserRegistrationService
{
    private readonly AppDbContext _dbContext;
    private readonly UsersOptions _usersOptions;
    private readonly PasswordHelper _passwordHelper;

    public UserRegistrationService(AppDbContext dbContext, IOptions<UsersOptions> usersOptions, PasswordHelper passwordHelper)
    {
        _dbContext = dbContext;
        _usersOptions = usersOptions.Value;
        _passwordHelper = passwordHelper;
    }

    public async Task<User> Register(RegisterUser.Request request, CancellationToken cancellationToken, RoleType? role = RoleType.User)
    {
        var passwordHex = _passwordHelper.GetHashUsingArgon2(request.Password);
        var isExistAdmin = await _dbContext.Roles.AnyAsync(x => x.Name == RoleType.Administrator, cancellationToken);
        if (!isExistAdmin && request.Email == _usersOptions.AdministratorEmail && role != RoleType.Administrator)
        {
            role = RoleType.Administrator;
        }

        var user = ToUser(request.Email, request.DateOfBirth, passwordHex, role.Value);
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return user;
    }

    private static User ToUser(string email, DateTime? dateOfBirth, string passwordHex, RoleType role)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Password = passwordHex,
            Email = email,
            DateOfBirth = dateOfBirth.Value,
            RegisteredAt = DateTime.Now.ToUniversalTime(),
            Roles = new List<Role>()
            {
                new()
                {
                    Name = role,
                    CreatedAt = DateTime.Now.ToUniversalTime()
                }
            }
        };
    }
}