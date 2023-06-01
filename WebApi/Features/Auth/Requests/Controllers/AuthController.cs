using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<string> Authenticate(Authenticate.Request request, CancellationToken cancellationToken)
    {
        request.UserAgent = Request.Headers["User-Agent"].ToString();
        
        var result = await _mediator.Send(request, cancellationToken);
        
        HttpContext.Response.Cookies.Append("refreshToken", result.UserModel.RefreshToken, new CookieOptions { 
            Expires = DateTime.Now.AddDays(1), //todo refresh token date
            Path = "/api/test",
        } );
        
        return result.UserModel.AccessToken;
    }
}