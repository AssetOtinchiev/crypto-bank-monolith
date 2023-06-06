namespace WebApi.Features.Accounts.Errors.Codes;

public class AccountsValidationErrors
{
    private const string Prefix = "accounts_validation_";
    
    public const string AccountsAmountLow = Prefix + "accounts_amount_low";
    public const string AccountsCurrencyRequired = Prefix + "accounts_currency_required";
    public const string AccountsCurrencyMinLength = Prefix + "accounts_currency_min_length";
    public const string AccountsUserNotExist = Prefix + "accounts_user_not_exist";
    public const string AccountsLimitExceeded = Prefix + "accounts_limit_exceeded";
    
    public const string AccountsStartDateRequired = Prefix + "accounts_start_date_required";
    public const string AccountsEndDateRequired = Prefix + "accounts_end_date_required";
    public const string AccountsStartDateMoreEndDate = Prefix + "accounts_start_date_more_end_date";
}