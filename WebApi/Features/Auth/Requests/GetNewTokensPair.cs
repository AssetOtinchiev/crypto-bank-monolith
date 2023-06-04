using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Auth.Services;

namespace WebApi.Features.Auth.Requests;

public class GetNewTokensPair
{
    public record Request : IRequest<Response>
    {
        public string? DeviceName { get; set; }
        public string? RefreshToken { get; set; }
    };

    public record Response(string AccessToken, string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .WithMessage("Empty token");
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
            var refreshToken = _dbContext.RefreshTokens
                .Include(x => x.User)
                .FirstOrDefault(x => x.Token == request.RefreshToken
                                     && x.DeviceName == request.DeviceName);

            if (refreshToken == null)
            {
                throw new ValidationErrorsException($"{nameof(request.RefreshToken)}", "Invalid token", "");
            }

            if (refreshToken.IsRevoked || refreshToken.ExpiryDate <= DateTime.Now.ToUniversalTime())
            {
                var activeRefreshToken = await _dbContext.RefreshTokens.Where(x =>
                        x.UserId == refreshToken.UserId
                        && x.DeviceName == request.DeviceName
                        && !x.IsRevoked
                    )
                    .FirstOrDefaultAsync(cancellationToken);

                if (activeRefreshToken != null)
                {
                    activeRefreshToken.IsRevoked = true;
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                throw new ValidationErrorsException($"{nameof(request.RefreshToken)}", "Invalid token", "");
            }

            var (accessToken, generatedRefreshToken) =
                await _tokenService.GenerateTokensAsync(refreshToken.User, request.DeviceName, cancellationToken);
            return new Response(accessToken, generatedRefreshToken);
        }
    }
}