using System.ComponentModel.DataAnnotations;

namespace WebApi.Features.Auth.Models;

public class AuthenticateModel
{
    [DataType(DataType.EmailAddress)]
    public string Email { get; set; }
    
    public string Password { get; set; }
}