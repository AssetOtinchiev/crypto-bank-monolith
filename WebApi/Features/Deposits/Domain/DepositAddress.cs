using WebApi.Features.Users.Domain;

namespace WebApi.Features.Deposits.Domain;

public class DepositAddress
{
    public int Id { get; set; }
    public string CurrencyCode { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public int XpubId { get; set; }
    public Xpub Xpub { get; set; } 
    public int DerivationIndex { get; set; }
    public string CryptoAddress { get; set; }
}