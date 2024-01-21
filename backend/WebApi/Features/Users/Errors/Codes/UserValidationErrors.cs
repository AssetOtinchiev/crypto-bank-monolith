namespace WebApi.Features.Users.Errors.Codes;

public static class UserValidationErrors
{
    private const string Prefix = "users_validation_";

    public const string PasswordRequired = Prefix + "password_required";
    public const string PasswordShort = Prefix + "password_short";
    
    public const string DateBirthRequired = Prefix + "date_birth_required";
    
    public const string EmailRequired = Prefix + "email_required";
    public const string EmailShort = Prefix + "email_short";
    public const string EmailInvalidFormat = Prefix + "email_invalid_format";
    public const string EmailExistOrInvalid = Prefix + "email_exist_or_invalid";
    
    public const string NotExist = Prefix + "not_exist";
    public const string RoleRequired = Prefix + "role_required";
    
}