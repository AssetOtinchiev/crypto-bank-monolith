using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Services;
using WebApi.Shared;

namespace WebApi.Features.Auth.Requests;

public class GetNewRefreshToken
{
    public record Request(string AccessToken) : IRequest<Response>
    {
        public string? UserAgent { get; set; }
        public string? RefreshToken { get; set; }
    };

    public record Response(AccessTokenModel UserModel);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.AccessToken)
                .NotEmpty()
                .WithMessage("Empty token");

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Empty token");
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly AppDbContext _dbContext;
        private readonly TokenService _tokenService;
        private readonly PasswordHelper _passwordHelper;

        public RequestHandler(AppDbContext dbContext, TokenService tokenService, PasswordHelper passwordHelper)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
            _passwordHelper = passwordHelper;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var userId = await _tokenService.GetUserIdFromToken(request.AccessToken);
            var refreshTokens = await _dbContext.RefreshTokens
                .Where(x => x.UserId == userId && x.DeviceName == request.UserAgent)
                .OrderByDescending(x=> x.CreatedAt)
                .ToArrayAsync(cancellationToken);
            
            var exist = false;
            foreach (var refreshToken in refreshTokens)
            {
                var refreshTokenHashed = _passwordHelper.GetHashUsingArgon2(request.RefreshToken, Convert.FromBase64String(refreshToken.TokenSalt));
                if (refreshToken.TokenHash == refreshTokenHashed)
                {
                    exist = true;
                }
            }

            Console.WriteLine(exist);
            return new Response(new AccessTokenModel());
        }
    }
}