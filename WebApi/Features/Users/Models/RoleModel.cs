using WebApi.Features.Users.Domain;

namespace WebApi.Features.Users.Models;

public class RoleModel
{
    public Guid Id { get; set; }
    public RoleType Name { get; set; }
}