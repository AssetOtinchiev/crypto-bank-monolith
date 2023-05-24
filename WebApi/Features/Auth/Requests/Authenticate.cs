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

    public record Response(RefreshTokenModel UserModel);

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
                var isExistUser = await dbContext.Users.AnyAsync(user => user.Email == x, token);

                return isExistUser;
            }).WithMessage("Email not found");


            RuleFor(x => x.RegisterUserModel).MustAsync(async (x, token) =>
            {
                var user = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == x.Email, token);
                var passwordHash =
                    PasswordHelper.HashUsingPbkdf2(x.Password, Convert.FromBase64String(user.PasswordSalt));

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
            var user = _dbContext.Users.SingleOrDefault(user => user.Email == request.RegisterUserModel.Email);
            var token = await Task.Run(() => _tokenService.GenerateTokensAsync(user.Id, cancellationToken));

            var refreshTokenModel = new RefreshTokenModel
            {
                AccessToken = token.Item1,
                RefreshToken = token.Item2,
                UserId = user.Id,
                Email = user.Email
            };

            return new Response(refreshTokenModel);
        }
    }
}