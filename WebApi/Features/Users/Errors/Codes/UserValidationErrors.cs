namespace WebApi.Features.Users.Errors.Codes;

public static class UserValidationErrors
{
    private const string Prefix = "users_validation_";

    public const string UserPasswordRequired = Prefix + "user_password_required";
    public const string UserPasswordShort = Prefix + "user_password_short";
    public const string UserDateBirthRequired = Prefix + "user_date_birth_required";
}