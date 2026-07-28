using System.Text.Json.Serialization;

namespace HRestaurant.DTOS.Responses;

public class ApiResponse<T>
{
    protected internal ApiResponse(
        bool success,
        string message,
        T? data,
        IReadOnlyCollection<ErrorResponse>? errors,
        int statusCode,
        string? traceId = null,
        string? stackTrace = null)
    {
        if (statusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(
                nameof(statusCode),
                "Status code must be a valid HTTP status code.");
        }

        if (success && statusCode is not (>= 200 and <= 299))
        {
            throw new ArgumentException(
                "A successful response must have a 2xx status code.",
                nameof(statusCode));
        }

        if (!success && statusCode < 400)
        {
            throw new ArgumentException(
                "A failed response must have a 4xx or 5xx status code.",
                nameof(statusCode));
        }

        Success = success;
        Message = message ?? string.Empty;
        Data = data;
        Errors = errors ?? Array.Empty<ErrorResponse>();
        StatusCode = statusCode;
        TraceId = traceId;
        StackTrace = stackTrace;
    }

    public bool Success { get; }

    public string Message { get; }

    public T? Data { get; }

    public IReadOnlyCollection<ErrorResponse> Errors { get; }

    public int StatusCode { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? TraceId { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? StackTrace { get; }

    public ApiResponse<T> WithTraceId(string traceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(traceId);

        return new ApiResponse<T>(
            Success,
            Message,
            Data,
            Errors,
            StatusCode,
            traceId,
            StackTrace);
    }
}

public static class ApiResponse
{
    public static ApiResponse<T> Ok<T>(
        T data,
        string message = "Request completed successfully.")
    {
        return new ApiResponse<T>(
            true,
            message,
            data,
            Array.Empty<ErrorResponse>(),
            200);
    }

    public static ApiResponse<T> Created<T>(
        T data,
        string message = "Resource created successfully.")
    {
        return new ApiResponse<T>(
            true,
            message,
            data,
            Array.Empty<ErrorResponse>(),
            201);
    }

    public static ApiResponse<object?> Success(
        string message = "Request completed successfully.")
    {
        return new ApiResponse<object?>(
            true,
            message,
            null,
            Array.Empty<ErrorResponse>(),
            200);
    }

    public static ApiResponse<object?> NoContent(
        string message = "Request completed successfully.")
    {
        return new ApiResponse<object?>(
            true,
            message,
            null,
            Array.Empty<ErrorResponse>(),
            204);
    }

    public static ApiResponse<T> Failure<T>(
        int statusCode,
        string message,
        IEnumerable<ErrorResponse>? errors = null,
        string? traceId = null,
        string? stackTrace = null)
    {
        var errorList = errors?.ToArray();

        if (errorList is null || errorList.Length == 0)
        {
            errorList =
            [
                new ErrorResponse("request_failed", message)
            ];
        }

        return new ApiResponse<T>(
            false,
            message,
            default,
            errorList,
            statusCode,
            traceId,
            stackTrace);
    }

    public static ApiResponse<T> NotFound<T>(string resourceName)
    {
        var message = $"{resourceName} was not found.";

        return Failure<T>(
            404,
            message,
            [new ErrorResponse("not_found", message)]);
    }

    public static ApiResponse<T> PersistenceFailure<T>()
    {
        const string message = "The operation could not be persisted.";

        return Failure<T>(
            500,
            message,
            [new ErrorResponse("persistence_error", message)]);
    }
}
