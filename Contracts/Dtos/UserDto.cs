using Contracts.Dtos.Base;

namespace Contracts.Dtos;

public class UserDto : AuditableBaseEntityDto
{
    public string Email { get; set; }
}