using Domain.Entitites.Base;

namespace Domain.Entitites;

public class User :AuditableBaseEntity<Guid>
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string PasswordSalt { get; set; }
}