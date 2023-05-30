using System.ComponentModel.DataAnnotations;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;
using WebApi.Features.Users.Options;
using WebApi.Shared;
using WebApi.Validations;

namespace WebApi.Features.Users.Requests;

public static class RegisterUser
{
    public record Request(RegisterUserModel RegisterUserModel) : IRequest<Response>;

    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.RegisterUserModel.Password).ValidPassword();
            
            RuleFor(x => x.RegisterUserModel.DateOfBirth)
                .NotEmpty();

            RuleFor(x => x.RegisterUserModel.Email).ValidEmail();

            RuleFor(x => x.RegisterUserModel)
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Email == x.Email, token);
            
                    return !userExists;
                }).WithMessage("User already exists with the same email");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;
        private readonly UsersOptions _usersOptions; 

        public RequestHandler(AppDbContext dbContext, IOptions<UsersOptions> usersOptions)
        {
            _dbContext = dbContext;
            _usersOptions = usersOptions.Value;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var salt = PasswordHelper.GetSecureSalt();
            var passwordHex = PasswordHelper.GetHexUsingArgon2T(request.RegisterUserModel.Password, salt);
            var role = RoleType.User;
            var isExistAdmin = await _dbContext.Roles.AnyAsync(x => x.Name == RoleType.Administrator, cancellationToken);
            if (!isExistAdmin && request.RegisterUserModel.Email == _usersOptions.AdministratorEmail)
            {
                role = RoleType.Administrator;
            }
            
            var user = ToUser(request.RegisterUserModel, passwordHex, role);
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(ToUserModel(user));
        }
    }

    private static User ToUser(RegisterUserModel registerUserModel, string passwordHex, RoleType role)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Password = passwordHex,
            Email = registerUserModel.Email,
            DateOfBirth = registerUserModel.DateOfBirth.Value,
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

    private static UserModel ToUserModel(User user)
    {
        return new UserModel()
        {
            Id = user.Id,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            DateOfRegistration = user.RegisteredAt
        };
    }
}