using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApi.Features.Auth.Models;

namespace WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<AccessTokenModel> Authenticate(Authenticate.Request request, CancellationToken cancellationToken)
    {
       var result = await _mediator.Send(request, cancellationToken);
       return result.UserModel;
    }
}