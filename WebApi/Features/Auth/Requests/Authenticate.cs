using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Services;
using WebApi.Shared;

namespace WebApi.Features.Auth.Requests;

public static class Authenticate
{
    public record Request(AuthenticateModel RegisterUserModel) : IRequest<Response>;

    public record Response(AccessTokenModel UserModel);

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
                var userExists = await dbContext.Users.AnyAsync(user => user.Email == x, token);

                return userExists;
            }).WithMessage("Email not found");


            RuleFor(x => x.RegisterUserModel).MustAsync(async (x, token) =>
            {
                var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == x.Email, token);
                var passwordHash =
                    PasswordHelper.HashUsingArgon2(x.Password, Convert.FromBase64String(user.PasswordSalt));

                if (user.Password != passwordHash)
                {
                    return false;
                }

                return true;
            }).WithMessage("Invalid Password");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;
        private readonly TokenService _tokenService;

        public RequestHandler(AppDbContext dbContext, TokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = _dbContext.Users
                .Include(x=> x.Roles)
                .SingleOrDefault(user => user.Email == request.RegisterUserModel.Email);
            var token = await _tokenService.GenerateTokensAsync(user, cancellationToken);

            var refreshTokenModel = new AccessTokenModel
            {
                AccessToken = token.Item1
            };

            return new Response(refreshTokenModel);
        }
    }
}