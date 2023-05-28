namespace WebApi.Features.Users.Models;

public class UserModel
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    
    public DateTime DateOfRegistration { get; set; } = DateTime.Now.ToUniversalTime();
    public DateTime DateOfBirth { get; set; }
}