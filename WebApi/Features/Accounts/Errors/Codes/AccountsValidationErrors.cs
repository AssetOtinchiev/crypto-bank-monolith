namespace WebApi.Features.Accounts.Errors.Codes;

public class AccountsValidationErrors
{
    private const string Prefix = "accounts_validation_";
    
    public const string AmountLow = Prefix + "amount_low";
    public const string CurrencyRequired = Prefix + "currency_required";
    public const string CurrencyMinLength = Prefix + "currency_min_length";
    public const string UserNotExist = Prefix + "user_not_exist";
    public const string LimitExceeded = Prefix + "limit_exceeded";
    
    public const string StartDateRequired = Prefix + "start_date_required";
    public const string EndDateRequired = Prefix + "end_date_required";
    public const string StartDateMoreEndDate = Prefix + "start_date_more_end_date";
}