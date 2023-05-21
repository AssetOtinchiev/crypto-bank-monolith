using Domain.Entitites.Base;

namespace Domain.Entitites;

public class User :AuditableBaseEntity<Guid>
{
    public User()
    {
        RefreshTokens = new HashSet<RefreshToken>();
    }
    public string Email { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    
  
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}