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
    public async Task<AccountModel> CreateAccount(CreateAccount.Request request, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new CreateAccount.Request(request.UserId, request.Currency, request.Amount), cancellationToken);
        return response.AccountModel;
    }
    
    
    [HttpGet]
    [Authorize]
    public async Task<AccountModel[]> GetAccounts(Guid userId, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new GetAccounts.Request(userId), cancellationToken);
        return response.AccountModels;
    }
    
    [HttpGet("period")]
    [Authorize(Roles = nameof(RoleType.Analyst))]
    public async Task<GetAccountOpenedByPeriodModel[]> GetAccountOpenedByPeriod(DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var response = await _dispatcher.Dispatch(new GetAccountOpenedByPeriod.Request(startDate, endDate), cancellationToken);
        return response.GetAccountOpenedByPeriodModel;
    }
}