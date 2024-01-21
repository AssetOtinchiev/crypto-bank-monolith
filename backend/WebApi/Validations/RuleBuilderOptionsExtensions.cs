using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebApi.Database;

namespace WebApi.Validations;

using static WebApi.Features.Auth.Errors.Codes.AuthValidationErrors;
using static WebApi.Errors.Codes.GeneralValidationErrors;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty()
            .WithErrorCode(InvalidCredential)
            .MinimumLength(4)
            .WithErrorCode(InvalidCredential)
            .EmailAddress()
            .WithErrorCode(InvalidCredential);
    }
    
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty()
            .WithErrorCode(InvalidCredential)
            .MinimumLength(7)
            .WithErrorCode(InvalidCredential);
    }


    public static IRuleBuilderOptions<T, Guid> UserExist<T>(this IRuleBuilder<T, Guid> builder, AppDbContext dbContext)
    {
        return builder.NotEmpty().MustAsync(async (x, token) =>
        {
            var userExists = await dbContext.Users.AnyAsync(user => user.Id == x, token);

            return userExists;
        }).WithErrorCode(UserNotExist);
    }
}