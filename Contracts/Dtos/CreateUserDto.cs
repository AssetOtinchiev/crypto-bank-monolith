using System.ComponentModel.DataAnnotations;

namespace Contracts.Dtos;

public class CreateUserDto
{
    [Required]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
  //  [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "E-mail is not valid")]
    public string Email { get; set; }
    
    [Required]
    public string Password { get; set; }
}