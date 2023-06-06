namespace WebApi.Features.Auth.Errors.Codes;

public class AuthValidationErrors
{
    private const string Prefix = "auth_validation_";
    
    public const string AuthInvalidCredential = Prefix + "auth_invalid_credential";
    public const string AuthTokenRequired = Prefix + "auth_token_required";
    public const string AuthTokenInvalid = Prefix + "auth_token_invalid";
}