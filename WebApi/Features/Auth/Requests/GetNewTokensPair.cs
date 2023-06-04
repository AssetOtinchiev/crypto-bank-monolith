using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Auth.Services;
using WebApi.Shared;

namespace WebApi.Features.Auth.Requests;

public class GetNewTokensPair
{
    public record Request(string AccessToken) : IRequest<Response>
    {
        public string? UserAgent { get; set; }
        public string? RefreshToken { get; set; }
    };

    public record Response(string AccessToken, string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
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
            var user = await _dbContext.Users
                .Include(x=> x.RefreshTokens
                    .Where(refreshToken => refreshToken.UserId == userId && refreshToken.DeviceName == request.UserAgent)
                    .OrderByDescending(refreshToken=> refreshToken.CreatedAt))
                .FirstAsync(x=> x.Id == userId, cancellationToken: cancellationToken);

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => !x.IsRevoked);
            if (activeRefreshToken == null)
            {
                throw new ValidationErrorsException($"{nameof(request.RefreshToken)}", "Invalid token","");
            }
            
            var refreshTokenParams = _passwordHelper.GetSettingsFromHexArgon2(activeRefreshToken.TokenHash);
            var refreshTokenHashed = _passwordHelper.HashUsingArgon2WithDbParam(request.RefreshToken, Convert.FromBase64String(refreshTokenParams.Salt), 
                refreshTokenParams.DegreeOfParallelism, refreshTokenParams.Iterations, refreshTokenParams.MemorySize);
            
            if (refreshTokenParams.Hash != refreshTokenHashed)
            {
                foreach (var refreshTokenOnRevorke in user.RefreshTokens)
                {
                    refreshTokenOnRevorke.IsRevoked = true;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new ValidationErrorsException($"{nameof(request.RefreshToken)}", "Invalid token","");
            }

            var token = await _tokenService.GenerateTokensAsync(user, request.UserAgent, cancellationToken);
            return new Response(token.Item1, token.Item2);
        }
    }
}