using WebApi.Features.Auth.Domain;

namespace WebApi.Features.Users.Domain;

public class User
{
    public User()
    {
        RefreshTokens = new HashSet<RefreshToken>();
    }
    
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime UpdatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime? DeletedAt { get; set; }

    public string Email { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
    
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
}