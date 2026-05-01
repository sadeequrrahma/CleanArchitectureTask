using System.Text;

namespace CleanArchitectureTask.Middleware;

/// <summary>
/// Microsoft.OpenApi 2.x + Swashbuckle can serialize operation security requirement keys as
/// "#/components/securitySchemes/Bearer" instead of "Bearer". Swagger UI then does not match
/// the scheme and sends requests without Authorization (401). Rewrite the JSON document.
/// </summary>
public sealed class SwaggerBearerSecurityKeyNormalizationMiddleware
{
    private readonly RequestDelegate _next;

    private const string WrongKey = "\"#/components/securitySchemes/Bearer\"";
    private const string CorrectKey = "\"Bearer\"";

    public SwaggerBearerSecurityKeyNormalizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.Equals("/swagger/v1/swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var buffer = new MemoryStream();
        context.Response.Body = buffer;

        await _next(context);

        buffer.Position = 0;
        var json = await new StreamReader(buffer).ReadToEndAsync();

        if (json.Contains(WrongKey, StringComparison.Ordinal))
            json = json.Replace(WrongKey, CorrectKey, StringComparison.Ordinal);

        context.Response.Body = originalBody;
        context.Response.Headers.ContentLength = Encoding.UTF8.GetByteCount(json);
        await context.Response.WriteAsync(json);
    }
}
