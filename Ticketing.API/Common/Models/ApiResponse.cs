namespace Ticketing.API.Common.Models;

public class ApiResponse<T>
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();
    public string? TraceId { get; init; }

    public static ApiResponse<T> Success(T data, string message = "Request completed successfully.") =>
        new()
        {
            Succeeded = true,
            Message = message,
            Data = data
        };

    public static ApiResponse<T> Failure(IEnumerable<string> errors, string message = "Request failed.", string? traceId = null) =>
        new()
        {
            Succeeded = false,
            Message = message,
            Errors = errors.ToList(),
            TraceId = traceId
        };
}
