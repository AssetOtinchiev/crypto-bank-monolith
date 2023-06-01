using WebApi.Features.Accounts.Domain;
using WebApi.Features.Auth.Domain;

namespace WebApi.Features.Users.Domain;

public class User
{
    public User()
    {
        RefreshTokens = new HashSet<RefreshToken>();
        Roles = new HashSet<Role>();
    }
    
    public Guid Id { get; set; }
    public DateTime RegisteredAt { get; set; }
    public DateTime DateOfBirth { get; set; }

    public string Email { get; set; }
    public string Password { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    public virtual ICollection<Role> Roles { get; set; }
    
    public virtual ICollection<Account> Accounts { get; set; }
}