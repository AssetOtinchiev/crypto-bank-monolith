namespace WebApi.Features.Accounts.Errors.Codes;

public class AccountLogicConflictErrors
{
    private const string Prefix = "accounts_logic_confict_";
    
    public const string LimitExceeded = Prefix + "limit_exceeded";
}