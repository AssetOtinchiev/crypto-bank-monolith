namespace WebApi.Features.Accounts.Models;

public class CreateAccountModel
{
    public Guid UserId { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
}