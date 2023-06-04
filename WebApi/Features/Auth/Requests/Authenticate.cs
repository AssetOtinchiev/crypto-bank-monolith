using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Auth.Services;
using WebApi.Shared;
using WebApi.Validations;

namespace WebApi.Features.Auth.Requests;

public static class Authenticate
{
    public record Request(string Email, string Password) : IRequest<Response>
    {
        public string? UserAgent { get; set; }
    };

    public record Response(string AccessToken, string RefreshToken);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator(AppDbContext dbContext)
        {
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleFor(x => x.Email).ValidEmail();

            RuleFor(x => x.Password).ValidPassword();
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
            var user = _dbContext.Users
                .Include(x=> x.Roles)
                .Include(x=> x.RefreshTokens)
                .SingleOrDefault(user => user.Email == request.Email);

            if (user == null)
            {
                throw new ValidationErrorsException($"{nameof(request.Email)}", "Invalid credentials","");
            }

            var passwordParam = _passwordHelper.GetSettingsFromHexArgon2(user.Password);
            var passwordHash =
                _passwordHelper.HashUsingArgon2WithDbParam(request.Password, Convert.FromBase64String(passwordParam.Salt),
                    passwordParam.DegreeOfParallelism, passwordParam.Iterations, passwordParam.MemorySize);
            if (passwordParam.Hash != passwordHash)
            {
                
                throw new ValidationErrorsException($"{nameof(request.Email)}", "Invalid credentials","");
            }
            
            var token = await _tokenService.GenerateTokensAsync(user, request.UserAgent, cancellationToken);
            return new Response(token.accessToken, token.refreshToken);
        }
    }
}