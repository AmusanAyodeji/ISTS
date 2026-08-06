using System.Net;
using FluentValidation;
using Ticketing.API.Common.Models;

namespace Ticketing.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized request. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure(["Unauthorized request."], "Authentication failed.", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Requested resource was not found. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure([ex.Message], "Resource not found.", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation error occurred. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure(ex.Errors.Select(x => x.ErrorMessage), "Validation failed.", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Business rule violation. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure([ex.Message], "Business rule validation failed.", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred. TraceId: {TraceId}", context.TraceIdentifier);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = ApiResponse<object>.Failure(["An unexpected error occurred."], "Internal server error.", context.TraceIdentifier);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
