using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;

namespace CleanArchitectureTask.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = MapException(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var body = new ErrorEnvelope
        {
            Success = false,
            Message = message,
            Errors = errors
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static (HttpStatusCode StatusCode, string Message, List<string> Errors) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation failed.",
                validationException.Errors.Select(e => e.ErrorMessage).ToList()),

            UnauthorizedAccessException unauthorized => (
                HttpStatusCode.Unauthorized,
                unauthorized.Message,
                new List<string>()),

            ArgumentException argument => (
                HttpStatusCode.BadRequest,
                argument.Message,
                new List<string>()),

            InvalidOperationException io when io.Message.Contains("Azure Blob Storage is not configured", StringComparison.Ordinal) => (
                HttpStatusCode.ServiceUnavailable,
                io.Message,
                new List<string>()),

            InvalidOperationException invalidOp => (
                HttpStatusCode.Conflict,
                invalidOp.Message,
                new List<string>()),

            KeyNotFoundException keyNotFound => (
                HttpStatusCode.NotFound,
                keyNotFound.Message,
                new List<string>()),

            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected error occurred.",
                new List<string>())
        };
    }

    private sealed class ErrorEnvelope
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
    }
}
