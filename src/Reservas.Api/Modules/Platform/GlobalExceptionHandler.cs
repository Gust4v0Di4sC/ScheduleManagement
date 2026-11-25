using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Reservas.Api.Modules.Platform.Domain;

namespace Reservas.Api.Modules.Platform;

internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title, detail, code) = exception switch
        {
            DomainRuleException domainException => (
                StatusCodes.Status422UnprocessableEntity,
                "A business rule rejected the request.",
                domainException.Message,
                domainException.Code),
            ArgumentException => (
                StatusCodes.Status400BadRequest,
                "The request is invalid.",
                exception.Message,
                "request_invalid"),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "The operation is forbidden.",
                "The current identity cannot perform this operation.",
                "forbidden"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred.",
                "The request could not be completed.",
                "unexpected_error")
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "Unhandled exception while processing request {TraceId}.", httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(exception, "Request {TraceId} was rejected with code {ErrorCode}.", httpContext.TraceIdentifier, code);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Extensions = { ["code"] = code }
            }
        }).ConfigureAwait(false);
    }
}
