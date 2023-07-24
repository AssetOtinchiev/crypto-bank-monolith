namespace WebApi.Features.Deposits.Errors.Codes;

public class DepositValidationErrors
{
    private const string Prefix = "deposit_validation_";
    
    public const string InvalidCurrencyCode = Prefix + "invalid_currency_code";
    
    public const string XPubCurrencyCodeNotExist = Prefix + "xpub_currency_code_not_exist";
}