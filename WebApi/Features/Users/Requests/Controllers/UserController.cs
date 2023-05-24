using Microsoft.AspNetCore.Mvc;
using WebApi.Features.Users.Models;
using WebApi.Pipeline;

namespace WebApi.Features.Users.Requests.Controllers;

[ApiController]
[Route("/users")]
public class UserController : Controller
{
    private readonly Dispatcher _dispatcher;

    public UserController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<UserModel> GetDeposits(RegisterUserModel registerUserModel, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new RegisterUser.Request(registerUserModel), cancellationToken);
        return response.UserModel;
    }
    
}
