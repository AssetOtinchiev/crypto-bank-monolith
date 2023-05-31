using System.Security.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Features.Accounts.Models;
using WebApi.Features.Users.Domain;
using WebApi.Pipeline;

namespace WebApi.Features.Accounts.Requests.Controllers;

[ApiController]
[Route("/accounts")]
public class AccountController : Controller
{
    private readonly Dispatcher _dispatcher;

    public AccountController(Dispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    [HttpPost]
    [Authorize]
    public async Task<AccountModel> CreateAccount([FromBody] CreateAccount.Request request,
        CancellationToken cancellationToken)
    {
        var response =
            await _dispatcher.Dispatch(new CreateAccount.Request(request.UserId, request.Currency, request.Amount),
                cancellationToken);
        return response.AccountModel;
    }

    [HttpGet]
    [Authorize]
    public async Task<AccountModel[]> GetAccounts(CancellationToken cancellationToken)
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

        var response = await _dispatcher.Dispatch(new GetAccounts.Request(userId), cancellationToken);
        return response.AccountModels;
    }

    [HttpGet("period")]
    [Authorize(Roles = nameof(RoleType.Analyst))]
    public async Task<GetAccountOpenedByPeriodModel[]> GetAccountOpenedByPeriod(DateTime startDate, DateTime endDate,
        CancellationToken cancellationToken)
    {
        var response =
            await _dispatcher.Dispatch(new GetAccountOpenedByPeriod.Request(startDate, endDate), cancellationToken);
        return response.GetAccountOpenedByPeriodModel;
    }
}