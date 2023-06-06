namespace WebApi.Features.Users.Errors.Codes;

public static class UserValidationErrors
{
    private const string Prefix = "users_validation_";

    public const string UserPasswordRequired = Prefix + "user_password_required";
    public const string UserPasswordShort = Prefix + "user_password_short";
    
    public const string UserDateBirthRequired = Prefix + "user_date_birth_required";
    
    public const string UserEmailRequired = Prefix + "user_email_required";
    public const string UserEmailShort = Prefix + "user_email_short";
    public const string UserEmailInvalidFormat = Prefix + "user_email_invalid_format";
    public const string UserEmailExistOrInvalid = Prefix + "user_email_exist_or_invalid";
    
    public const string UserNotExist = Prefix + "user_not_exist";
    public const string UserRoleRequired = Prefix + "user_role_required";
    
}