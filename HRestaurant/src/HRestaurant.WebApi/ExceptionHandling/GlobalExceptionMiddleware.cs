using FluentValidationException = FluentValidation.ValidationException;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using ApplicationValidationException = HRestaurant.Exceptions.ValidationException;

namespace HRestaurant.WebApi.ExceptionHandling;

public sealed class GlobalExceptionMiddleware
{
    private const string TraceIdHeaderName = "X-Trace-Id";
    private const string UnexpectedErrorMessage =
        "An unexpected server error occurred.";

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(environment);

        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.Headers[TraceIdHeaderName] =
            GetTraceId(httpContext);

        try
        {
            await _next(httpContext);
        }
        catch (OperationCanceledException exception)
            when (httpContext.RequestAborted.IsCancellationRequested)
        {
            _logger.LogInformation(
                exception,
                "HTTP request was cancelled by the client. "
                + "Method: {RequestMethod}, Path: {RequestPath}, "
                + "TraceId: {TraceId}",
                httpContext.Request.Method,
                GetSafeRequestPath(httpContext),
                GetTraceId(httpContext));
        }
        catch (Exception exception)
        {
            if (httpContext.Response.HasStarted)
            {
                _logger.LogError(
                    exception,
                    "An exception occurred after the response started. "
                    + "Method: {RequestMethod}, Path: {RequestPath}, "
                    + "TraceId: {TraceId}",
                    httpContext.Request.Method,
                    GetSafeRequestPath(httpContext),
                    GetTraceId(httpContext));

                throw;
            }

            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext httpContext,
        Exception exception)
    {
        var traceId = GetTraceId(httpContext);
        var details = MapException(exception);

        LogException(httpContext, exception, details.StatusCode, traceId);

        var response = ApiResponse.Failure<object?>(
            details.StatusCode,
            details.Message,
            details.Errors,
            traceId,
            _environment.IsDevelopment()
                ? exception.ToString()
                : null);

        httpContext.Response.Clear();
        httpContext.Response.StatusCode = details.StatusCode;
        httpContext.Response.Headers[TraceIdHeaderName] = traceId;
        httpContext.Response.Headers.CacheControl = "no-store";

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken: CancellationToken.None);
    }

    private void LogException(
        HttpContext httpContext,
        Exception exception,
        int statusCode,
        string traceId)
    {
        const string message =
            "Exception handled for {RequestMethod} {RequestPath}. "
            + "StatusCode: {StatusCode}, TraceId: {TraceId}, "
            + "ExceptionType: {ExceptionType}";

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                message,
                httpContext.Request.Method,
                GetSafeRequestPath(httpContext),
                statusCode,
                traceId,
                exception.GetType().FullName);

            return;
        }

        _logger.LogWarning(
            exception,
            message,
            httpContext.Request.Method,
            GetSafeRequestPath(httpContext),
            statusCode,
            traceId,
            exception.GetType().FullName);
    }

    private static ExceptionDetails MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException notFound => CreateDetails(
                StatusCodes.Status404NotFound,
                "not_found",
                notFound.Message),

            ApplicationValidationException validation => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                validation.Message,
                CreateValidationErrors(validation.Errors)),

            FluentValidationException validation => new ExceptionDetails(
                StatusCodes.Status400BadRequest,
                validation.Message,
                validation.Errors
                    .Select(error => new ErrorResponse(
                        "validation_error",
                        error.ErrorMessage,
                        error.PropertyName))
                    .Distinct()
                    .ToArray()),

            ConflictException conflict => CreateDetails(
                StatusCodes.Status409Conflict,
                "conflict",
                conflict.Message),

            UnauthorizedException unauthorized => CreateDetails(
                StatusCodes.Status401Unauthorized,
                "unauthorized",
                unauthorized.Message),

            ForbiddenException forbidden => CreateDetails(
                StatusCodes.Status403Forbidden,
                "forbidden",
                forbidden.Message),

            BadHttpRequestException badRequest => CreateDetails(
                badRequest.StatusCode,
                "bad_request",
                badRequest.Message),

            _ => CreateDetails(
                StatusCodes.Status500InternalServerError,
                "internal_server_error",
                UnexpectedErrorMessage)
        };
    }

    private static ExceptionDetails CreateDetails(
        int statusCode,
        string errorCode,
        string message)
    {
        return new ExceptionDetails(
            statusCode,
            message,
            [new ErrorResponse(errorCode, message)]);
    }

    private static IReadOnlyCollection<ErrorResponse> CreateValidationErrors(
        IReadOnlyDictionary<string, string[]> validationErrors)
    {
        var errors = validationErrors
            .SelectMany(error => error.Value.Select(message =>
                new ErrorResponse(
                    "validation_error",
                    message,
                    error.Key)))
            .Distinct()
            .ToArray();

        return errors.Length > 0
            ? errors
            :
            [
                new ErrorResponse(
                    "validation_error",
                    "One or more validation errors occurred.")
            ];
    }

    private static string GetTraceId(HttpContext httpContext)
    {
        return httpContext.TraceIdentifier;
    }

    private static string GetSafeRequestPath(
        HttpContext httpContext)
    {
        return httpContext.Request.Path.StartsWithSegments(
            "/api/public/reservations/track",
            StringComparison.OrdinalIgnoreCase)
            ? "/api/public/reservations/track/{trackingToken}"
            : httpContext.Request.Path.Value ?? string.Empty;
    }

    private sealed record ExceptionDetails(
        int StatusCode,
        string Message,
        IReadOnlyCollection<ErrorResponse> Errors);
}
