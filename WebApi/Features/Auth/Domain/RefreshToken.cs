using WebApi.Features.Users.Domain;

namespace WebApi.Features.Auth.Domain;

public class RefreshToken
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public string TokenSalt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public string DeviceName { get; set; }
    public  Guid? ReplacedBy { get; set; }
    
    public virtual User User { get; set; }
}