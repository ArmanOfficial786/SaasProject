using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Application.Exceptions;
using Shared.Domain.Constants;

namespace Shared.Infrastructure.GlobalException;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, body) = exception switch
        {
            ValidationException vex => (
                (int)HttpStatusCode.BadRequest,
                Response<object>.FailureResponse(
                    "One or more validation failures have occurred.",
                    vex.Errors
                        .SelectMany(kvp => kvp.Value.Select(msg => new ErrorDTO(kvp.Key, msg)))
                        .ToArray())
            ),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                Response<object>.FailureResponse(
                    "An unexpected error occurred.",
                    Errors.Exception(exception))
            )
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
        }

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(body, cancellationToken);

        return true; // handled — stop the chain here
    }
}
