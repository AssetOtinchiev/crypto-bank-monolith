namespace WebApi.Features.Accounts.Models;

public class AccountModel
{
    public long Id { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateOfOpening { get; set; }

    public Guid UserId { get; set; }
}