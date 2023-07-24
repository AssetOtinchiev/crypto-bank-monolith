using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Features.Deposits.Requests.Controllers;

[ApiController]
[Route("/deposit")]
public class DepositController : Controller
{
    private readonly IMediator _mediator;

    public DepositController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet("roles")]
    [Authorize]
    public async Task<object> GetUserRoles([FromQuery] GetDepositAddress.Request request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.Send(request, cancellationToken);
        return response;
    }
}