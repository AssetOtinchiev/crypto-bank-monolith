using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;
using WebApi.Features.Users.Services;

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
            RuleFor(x => x.RegisterUserModel.Email)
                .MinimumLength(4)
                .EmailAddress();

            RuleFor(x => x.RegisterUserModel.Password)
                .MinimumLength(7);

            RuleFor(x => x.RegisterUserModel.Email).MustAsync(async (x, token) =>
            {
                var isExistUser = await dbContext.Users.AnyAsync(user => user.Email == x);
            
                return !isExistUser;
            }).WithMessage("User already exists with the same email");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;

        public RequestHandler(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var salt = PasswordHelper.GetSecureSalt();
            var passwordHash = PasswordHelper.HashUsingPbkdf2(request.RegisterUserModel.Password, salt);

            var user = ToUser(request.RegisterUserModel, passwordHash, Convert.ToBase64String(salt));
            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return new Response(ToUserDto(user));
        }
    }

    public static User ToUser(RegisterUserModel registerUserModel, string passwordHash, string passwordSalt)
    {
        return new User()
        {
            Id = Guid.NewGuid(),
            Password = passwordHash,
            PasswordSalt = passwordSalt,
            Email = registerUserModel.Email
        };
    }

    public static UserModel ToUserDto(User user)
    {
        return new UserModel()
        {
            Id = user.Id,
            Email = user.Email,
        };
    }
}