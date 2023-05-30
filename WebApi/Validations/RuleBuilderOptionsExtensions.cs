using FluentValidation;

namespace WebApi.Validations;

public static class RuleBuilderOptionsExtensions
{
    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .MinimumLength(4)
            .WithMessage("Invalid credentials")
            .EmailAddress()
            .WithMessage("Invalid credentials");
    }
    
    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> builder)
    {
        return builder
            .MinimumLength(7)
            .WithMessage("Invalid credentials");
    }
}