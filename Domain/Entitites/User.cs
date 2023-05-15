using Domain.Entitites.Base;

namespace Domain.Entitites;

public class User :AuditableBaseEntity<Guid>
{
    public string UserName { get; set; }
    public string Password { get; set; }
}