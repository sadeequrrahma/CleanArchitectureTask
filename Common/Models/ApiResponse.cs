namespace CleanArchitectureTask.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();

    public static ApiResponse<T> Ok(T data, string message = "")
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Errors = Array.Empty<string>()
        };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Data = default,
            Errors = errors ?? Array.Empty<string>()
        };
}

/// <summary>
/// Non-generic envelope for middleware and validation responses where no payload is returned.
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public IEnumerable<string> Errors { get; set; } = Array.Empty<string>();
}
