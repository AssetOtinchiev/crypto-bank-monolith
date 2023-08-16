using WebApi.Features.Accounts.Domain;
using WebApi.Features.Auth.Domain;
using WebApi.Features.Deposits.Domain;

namespace WebApi.Features.Users.Domain;

public class User
{
    public User()
    {
        RefreshTokens = new List<RefreshToken>();
        Roles = new HashSet<Role>();
        Accounts = new List<Account>();
        DepositAddresses = new List<DepositAddress>();
    }
    
    public Guid Id { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }
    public string Password { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; }
    public ICollection<Role> Roles { get; set; }
    
    public List<Account> Accounts { get; set; }
    
    public List<DepositAddress> DepositAddresses { get; set; }
}