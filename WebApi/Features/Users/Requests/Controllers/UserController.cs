using System.Security.Authentication;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Features.Users.Domain;
using WebApi.Features.Users.Models;

namespace WebApi.Features.Users.Requests.Controllers;

[ApiController]
[Route("/users")]
public class UserController : Controller
{

    private readonly IMediator _mediator;

    public UserController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<UserModel> RegisterUser([FromBody]RegisterUser.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.UserModel;
    }

    [HttpGet]
    [Authorize]
    public async Task<UserModel> GetUserProfile(CancellationToken cancellationToken)
    {
        var user = HttpContext?.User;

        Guid userId = Guid.Empty;
        if (user != null && user.Claims.Any())
        {
            var claimUserId = user.Claims.FirstOrDefault(x => x.Type == "userid")?.Value;
            if (string.IsNullOrEmpty(claimUserId))
            {
                throw new AuthenticationException("User not exist");
            }

            if (!Guid.TryParse(claimUserId, out userId))
            {
                throw new AuthenticationException("Invalid user id");
            }
        }
        var response = await _mediator.Send(new GetUserProfile.Request(userId), cancellationToken);
        return response.UserModel;
    }
    
    [HttpGet("roles")]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public async Task<RoleModel[]> GetUserRoles([FromQuery]GetUserRoles.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.RoleModels;
    }
    
    [HttpPut("roles")]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public async Task<RoleModel[]> EditUserRoles([FromBody] EditUserRoles.Request request, CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response.UserModel;
    }
    
}
