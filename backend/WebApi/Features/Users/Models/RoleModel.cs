using WebApi.Features.Users.Domain;

namespace WebApi.Features.Users.Models;

public class RoleModel
{
    public Guid UserId { get; set; }
    public RoleType Name { get; set; }
}