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
    public record Request(string Email, string Password, DateTime? DateOfBirth) : IRequest<Response>;

    public record Response(UserModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password is empty")
                .MinimumLength(7)
                .WithMessage("Password too short");
            
            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithMessage("Date is empty");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is empty")
                .MinimumLength(4)
                .WithMessage("Email too short")
                .EmailAddress()
                .WithMessage("Email format is wrong")
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Email == x, token);

                    return !userExists;
                }).WithMessage("Email exists or incorrect email");
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
            var passwordHex = PasswordHelper.GetHexUsingArgon2T(request.Password, salt);
            var role = RoleType.User;
            var isExistAdmin = await _dbContext.Roles.AnyAsync(x => x.Name == RoleType.Administrator, cancellationToken);
            if (!isExistAdmin && request.Email == _usersOptions.AdministratorEmail)
            {
                role = RoleType.Administrator;
            }
            
            var user = ToUser(request.Email, request.DateOfBirth, passwordHex, role);
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(ToUserModel(user));
        }
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

    private static UserModel ToUserModel(User user)
    {
        return new UserModel()
        {
            Id = user.Id,
            Email = user.Email,
            DateOfBirth = user.DateOfBirth,
            RegisteredAt = user.RegisteredAt
        };
    }
}