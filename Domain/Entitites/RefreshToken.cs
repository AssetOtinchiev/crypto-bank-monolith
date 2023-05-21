using Domain.Entitites.Base;

namespace Domain.Entitites;

public class RefreshToken :AuditableBaseEntity<Guid>
{
    public Guid UserId { get; set; }
    public string TokenHash { get; set; }
    public string TokenSalt { get; set; }
    public DateTime ExpiryDate { get; set; }
    public virtual User User { get; set; }
}