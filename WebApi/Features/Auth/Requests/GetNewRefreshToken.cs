using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Errors.Exceptions;
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
            var user = await _dbContext.Users
                .Include(x=> x.RefreshTokens
                    .Where(x => x.UserId == userId && x.DeviceName == request.UserAgent)
                    .OrderByDescending(x=> x.CreatedAt))
                .FirstAsync(x=> x.Id == userId, cancellationToken);

            var activeRefreshToken = user.RefreshTokens.FirstOrDefault(x => !x.IsRevorked);
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
                    refreshTokenOnRevorke.IsRevorked = true;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new ValidationErrorsException($"{nameof(request.RefreshToken)}", "Invalid token","");
            }

            var token = await _tokenService.GenerateTokensAsync(user, request.UserAgent, cancellationToken);
            return new Response(new AccessTokenModel()
            {
                AccessToken = token.Item1,
                RefreshToken = token.Item2
            });
        }
    }
}