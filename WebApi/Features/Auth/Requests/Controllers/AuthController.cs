using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Features.Auth.Models;
using WebApi.Features.Auth.Options;

namespace WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly IMediator _mediator;
    private readonly AuthOptions _authOptions;
    private const string RefreshTokenPath = "/auth/refreshToken";

    public AuthController(IMediator mediator, IOptions<AuthOptions> authOptions)
    {
        _mediator = mediator;
        _authOptions = authOptions.Value;
    }

    [HttpPost]
    public async Task<AccessTokenModel> Authenticate(Authenticate.Request request, CancellationToken cancellationToken)
    {
        request.UserAgent = GetUserAgentFromHeader();;
        var result = await _mediator.Send(request, cancellationToken);
        AddRefreshTokenCookies(result.RefreshToken);
        
        return new AccessTokenModel()
        {
            AccessToken = result.AccessToken
        };
    }

    [HttpGet("newTokens")]
    public async Task<AccessTokenModel> GetNewTokensPair([FromBody] GetNewTokensPair.Request request,
        CancellationToken cancellationToken)
    {
        request.UserAgent = GetUserAgentFromHeader();
        var refreshToken = Request.Cookies["refreshToken"];
        request.RefreshToken = refreshToken;

        var result = await _mediator.Send(request, cancellationToken);
        AddRefreshTokenCookies(result.RefreshToken);

        return new AccessTokenModel()
        {
            AccessToken = result.AccessToken
        };
    }

    private string GetUserAgentFromHeader()
    {
        return Request.Headers["User-Agent"].ToString();
    }

    private void AddRefreshTokenCookies(string refreshToken)
    {
        HttpContext.Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddHours(_authOptions.RefreshTokenExpiration.Hours),
            Path = RefreshTokenPath
        });
    }
}