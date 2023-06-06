using FluentValidation;

namespace WebApi.Validations;

using static WebApi.Features.Auth.Errors.Codes.AuthValidationErrors;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty()
            .WithErrorCode(AuthInvalidCredential)
            .MinimumLength(4)
            .WithErrorCode(AuthInvalidCredential)
            .EmailAddress()
            .WithErrorCode(AuthInvalidCredential);
    }
    
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .NotEmpty()
            .WithErrorCode(AuthInvalidCredential)
            .MinimumLength(7)
            .WithErrorCode(AuthInvalidCredential);
    }
}