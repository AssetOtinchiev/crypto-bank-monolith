using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Errors.Exceptions;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Services;
using WebApi.Shared;
using WebApi.Validations;

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
            RuleFor(x => x.RegisterUserModel.Email).ValidEmail();

            RuleFor(x => x.RegisterUserModel.Password).ValidPassword();
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

            if (user == null)
            {
                throw new ValidationErrorsException($"{nameof(request.RegisterUserModel.Email)}", "Invalid credentials","");
            }
            
            var passwordHash =
                PasswordHelper.HashUsingArgon2(request.RegisterUserModel.Password, Convert.FromBase64String(user.PasswordSalt));
            if (user.Password != passwordHash)
            {
                
                throw new ValidationErrorsException($"{nameof(request.RegisterUserModel.Email)}", "Invalid credentials","");
            }
            
            var token = await _tokenService.GenerateTokensAsync(user, cancellationToken);

            var refreshTokenModel = new AccessTokenModel
            {
                AccessToken = token.Item1
            };

            return new Response(refreshTokenModel);
        }
    }
}