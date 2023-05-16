using System.ComponentModel.DataAnnotations;

namespace Contracts.Dtos;

public class CreateUserDto
{
    [Required]
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}