using HRestaurant.DTOS.Responses;
using Microsoft.AspNetCore.Diagnostics;

namespace HRestaurant.WebApi.ExceptionHandling;

public sealed class GlobalExceptionHandler :
    IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(exception);

        if (exception is OperationCanceledException
            && httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        if (httpContext.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "The response has already started for {Method} {Path}.",
                httpContext.Request.Method,
                httpContext.Request.Path);

            return false;
        }

        _logger.LogError(
            exception,
            "Unhandled exception while processing {Method} {Path}.",
            httpContext.Request.Method,
            httpContext.Request.Path);

        const string message =
            "An unexpected server error occurred.";

        var response = ApiResponse.Failure<object?>(
            StatusCodes.Status500InternalServerError,
            message,
            [new ErrorResponse("internal_server_error", message)]);

        httpContext.Response.StatusCode = response.StatusCode;

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }
}
