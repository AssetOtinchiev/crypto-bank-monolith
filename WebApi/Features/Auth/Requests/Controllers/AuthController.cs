using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WebApi.Features.Auth.Options;

namespace WebApi.Features.Auth.Requests.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : Controller
{
    private readonly IMediator _mediator;
    private readonly JWTSetting _jwtSetting;
    private const string RefreshTokenPath = "/auth/refreshToken";

    public AuthController(IMediator mediator, IOptions<JWTSetting> jwtSetting)
    {
        _mediator = mediator;
        _jwtSetting = jwtSetting.Value;
    }

    [HttpPost]
    public async Task<string> Authenticate(Authenticate.Request request, CancellationToken cancellationToken)
    {
        request.UserAgent = Request.Headers["User-Agent"].ToString();
        var result = await _mediator.Send(request, cancellationToken);
        HttpContext.Response.Cookies.Append("refreshToken", result.UserModel.RefreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(_jwtSetting.ExpirationRefreshToken),
            Path = RefreshTokenPath
        });
        
        return result.UserModel.AccessToken;
    }


    [HttpPost("refreshToken")]
    public async Task<string> GetNewRefreshToken([FromBody]GetNewRefreshToken.Request request,CancellationToken cancellationToken)
    {
        request.UserAgent = Request.Headers["User-Agent"].ToString();
        
        var refreshToken = Request.Cookies["refreshToken"];
        request.RefreshToken = refreshToken;
        
        var result = await _mediator.Send(request, cancellationToken);
        HttpContext.Response.Cookies.Append("refreshToken", result.UserModel.RefreshToken, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(_jwtSetting.ExpirationRefreshToken),
            Path = RefreshTokenPath
        });
        
        return result.UserModel.AccessToken;
    }
}