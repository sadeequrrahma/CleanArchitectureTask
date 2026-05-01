using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitectureTask.Swagger;

/// <summary>
/// Marks [Authorize] operations with the Bearer scheme so Swagger UI attaches the token from **Authorize**.
/// Without this, Try it out sends no Authorization header and APIs return 401.
/// </summary>
public sealed class JwtBearerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.MethodInfo;
        var declaring = method.DeclaringType;

        if (method.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any())
            return;

        if (declaring?.GetCustomAttributes(true).OfType<AllowAnonymousAttribute>().Any() == true)
            return;

        var requiresAuth = method.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any()
            || declaring?.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() == true;

        if (!requiresAuth)
            return;

        var bearerRef = new OpenApiSecuritySchemeReference("Bearer", context.Document, string.Empty);

        operation.Security =
        [
            new OpenApiSecurityRequirement { [bearerRef] = new List<string>() }
        ];
    }
}
