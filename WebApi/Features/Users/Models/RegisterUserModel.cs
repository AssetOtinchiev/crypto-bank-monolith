using System.ComponentModel.DataAnnotations;

namespace WebApi.Features.Users.Models;

public class RegisterUserModel
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    public string Password { get; set; }
}