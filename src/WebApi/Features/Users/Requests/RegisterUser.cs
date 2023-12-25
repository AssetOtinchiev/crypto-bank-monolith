using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;
using WebApi.Features.Users.Services;
using static WebApi.Features.Users.Errors.Codes.UserValidationErrors;

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
                .WithErrorCode(PasswordRequired)
                .MinimumLength(7)
                .WithErrorCode(PasswordShort);

            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .WithErrorCode(DateBirthRequired);

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithErrorCode(EmailRequired)
                .MinimumLength(4)
                .WithErrorCode(EmailShort)
                .EmailAddress()
                .WithErrorCode(EmailInvalidFormat)
                .MustAsync(async (x, token) =>
                {
                    var userExists = await dbContext.Users.AnyAsync(user => user.Email == x, token);

                    return !userExists;
                }).WithErrorCode(EmailExistOrInvalid);
        }
    }

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly UserRegistrationService _userRegistrationService;

        public RequestHandler(UserRegistrationService userRegistrationService)
        {
            _userRegistrationService = userRegistrationService;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var user = await _userRegistrationService.Register(request, cancellationToken);
            var userModel = ToUserModel(user);
            return new Response(userModel);
        }
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