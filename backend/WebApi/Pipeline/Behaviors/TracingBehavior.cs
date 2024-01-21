using MediatR;
using WebApi.Observability;

namespace WebApi.Pipeline.Behaviors;

public class TracingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var activity = Telemetry.ActivitySource.StartActivity($"Handling {request.GetType().FullName}");

        return next();
    }
}