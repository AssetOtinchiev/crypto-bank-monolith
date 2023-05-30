using System.ComponentModel;

namespace WebApi.Features.Users.Domain;

public class Role
{
    public RoleType Name { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    
    public virtual User User { get; set; }
}

public enum RoleType
{
    [Description("Administrator")]
    Administrator = 0,
    [Description("Analyst")]
    Analyst = 1,
    [Description("User")]
    User = 2,
}