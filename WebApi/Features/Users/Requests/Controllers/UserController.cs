using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Extensions;
using WebApi.Features.Users.Domain;
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
    public async Task<UserModel> RegisterUser(RegisterUserModel registerUserModel, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new RegisterUser.Request(registerUserModel), cancellationToken);
        return response.UserModel;
    }
    
    
    [HttpGet]
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

        var response = await _dispatcher.Dispatch(new GetUserProfile.Request(userId), cancellationToken);
        return response.UserModel;
    }
    
    [HttpGet("roles")]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public async Task<RoleModel[]> GetUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new GetUserRoles.Request(userId), cancellationToken);
        return response.RoleModels;
    }
    
    [HttpPut("roles")]
    [Authorize(Roles = nameof(RoleType.Administrator))]
    public async Task<RoleModel[]> EditUserRoles([FromBody] RoleType[] roleTypes,Guid userId, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new EditUserRoles.Request(roleTypes,userId), cancellationToken);
        return response.UserModel;
    }
    
}
