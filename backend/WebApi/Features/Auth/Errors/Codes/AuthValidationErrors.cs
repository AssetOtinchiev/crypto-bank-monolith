namespace WebApi.Features.Auth.Errors.Codes;

public class AuthValidationErrors
{
    private const string Prefix = "auth_validation_";
    
    public const string InvalidCredential = Prefix + "invalid_credential";
    public const string TokenRequired = Prefix + "token_required";
    public const string TokenInvalid = Prefix + "token_invalid";
}