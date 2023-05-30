using Microsoft.AspNetCore.Mvc;
using WebApi.Features.Auth.Models;
using WebApi.Pipeline;

namespace WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly Dispatcher _dispatcher;

    public AuthController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    public async Task<AccessTokenModel> Authenticate(AuthenticateModel authenticateModel, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new Authenticate.Request(authenticateModel), cancellationToken);
        return response.UserModel;
    }
}