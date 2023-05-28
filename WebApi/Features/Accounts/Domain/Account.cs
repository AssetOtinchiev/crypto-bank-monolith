using WebApi.Features.Users.Domain;

namespace WebApi.Features.Accounts.Domain;

public class Account
{
    public long Id { get; set; }
    public string Currency { get; set; }
    public decimal Amount { get; set; }
    public DateTime DateOfOpening { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }
}