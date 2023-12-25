using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebApi.Errors.Exceptions;

namespace WebApi.Errors.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder MapProblemDetails(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(builder =>
        {
            builder.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>()!;
                var exception = exceptionHandlerPathFeature.Error;

                switch (exception)
                {
                    case ValidationErrorsException validationErrorsException:
                    {
                        var validationProblemDetails = HandleProblemDetails("Validation failed",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/400",
                            400, validationErrorsException.Message, context);

                        validationProblemDetails.Extensions["errors"] = validationErrorsException.Errors
                            .Select(x => new ErrorDataWithCode(x.Field, x.Message, x.Code));

                        await context.Response.WriteAsync(JsonSerializer.Serialize(validationProblemDetails));
                        break;
                    }
                    case LogicConflictException logicConflictException:
                        var logicConflictProblemDetails = HandleProblemDetails("Logic conflict",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/422",
                            422, logicConflictException.Message, context);
                        logicConflictProblemDetails.Extensions["code"] = logicConflictException.Code;

                        await context.Response.WriteAsync(JsonSerializer.Serialize(logicConflictProblemDetails));
                        break;
                    case OperationCanceledException:
                        var operationCanceledProblemDetails = HandleProblemDetails("Timeout",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/504",
                            504, "Request timed out", context);

                        await context.Response.WriteAsync(JsonSerializer.Serialize(operationCanceledProblemDetails));
                        break;
                    default:
                        var internalErrorProblemDetails = HandleProblemDetails("Internal server error",
                            "https://developer.mozilla.org/en-US/docs/Web/HTTP/Status/500",
                            500, "Interval server error has occured", context);

                        await context.Response.WriteAsync(JsonSerializer.Serialize(internalErrorProblemDetails));
                        break;
                }
            });
        });

        return app;
    }

    private static ProblemDetails HandleProblemDetails(
        string title, string type, int status, string message,
        HttpContext context)
    {
        var validationProblemDetails = new ProblemDetails
        {
            Title = title,
            Type = type,
            Detail = message,
            Status = status,
        };

        validationProblemDetails.Extensions.Add("traceId",
            Activity.Current?.Id ?? context.TraceIdentifier);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = status;
        return validationProblemDetails;
    }
}

internal record ErrorDataWithCode(
    [property: JsonPropertyName("field")] string Field,
    [property: JsonPropertyName("message")]
    string Message,
    [property: JsonPropertyName("code")] string Code);