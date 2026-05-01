using System.Security.Claims;
using System.Text;
using CleanArchitectureTask.Application;
using CleanArchitectureTask.Common.Models;
using CleanArchitectureTask.Helpers;
using CleanArchitectureTask.Infrastructure;
using CleanArchitectureTask.Infrastructure.Data;
using CleanArchitectureTask.Middleware;
using CleanArchitectureTask.Swagger;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext();
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException($"Configuration section '{JwtSettings.SectionName}' is missing.");
if (jwtSettings.Secret.Length < 32)
    throw new InvalidOperationException("Jwt:Secret must be at least 32 characters for HS256.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        // Swagger UI uses scheme "bearer" and adds "Bearer " automatically. If you also paste
        // "Bearer eyJ...", the header becomes "Bearer Bearer eyJ..." and validation fails with 401.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers.Authorization.ToString();
                if (string.IsNullOrWhiteSpace(authHeader))
                    return Task.CompletedTask;

                const string bearerPrefix = "Bearer ";
                if (!authHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    return Task.CompletedTask;

                var remainder = authHeader.AsSpan(bearerPrefix.Length).Trim().ToString();

                while (remainder.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
                    remainder = remainder.Substring(bearerPrefix.Length).Trim();

                if (remainder.Length >= 2 && remainder.StartsWith('"') && remainder.EndsWith('"'))
                    remainder = remainder.Substring(1, remainder.Length - 2).Trim();

                if (remainder.Length > 0)
                    context.Token = remainder;

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (builder.Environment.IsDevelopment())
                    Log.Warning(context.Exception, "JWT authentication failed");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var appInsightsConnection =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
    ?? builder.Configuration["ApplicationInsights:ConnectionString"];
if (!string.IsNullOrWhiteSpace(appInsightsConnection))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
        options.ConnectionString = appInsightsConnection);
}

builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.Values
            .SelectMany(v => v.Errors)
            .Select(e => string.IsNullOrEmpty(e.ErrorMessage) ? "Invalid value." : e.ErrorMessage)
            .ToList();

        var response = new ApiResponse
        {
            Success = false,
            Message = "Validation failed.",
            Errors = errors
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CleanArchitectureTask API",
        Version = "v1",
        Description =
            "REST API. Login, copy **`data.accessToken`**, click **Authorize**, paste **only the token** (no `Bearer` prefix). Then call protected endpoints."
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste **only** the JWT from login/register (Swagger adds the Bearer prefix)."
    });

    options.OperationFilter<JwtBearerOperationFilter>();
});

var app = builder.Build();

Log.Information("ASPNETCORE_ENVIRONMENT = {Environment}", app.Environment.EnvironmentName);

await DbInitializer.SeedAsync(app.Services);

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

if (app.Environment.IsDevelopment())
{
    // Must run before UseSwagger so the OpenAPI JSON is rewritten before it reaches the client.
    app.UseMiddleware<SwaggerBearerSecurityKeyNormalizationMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
        options.EnablePersistAuthorization();
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();
}
else
{
    app.MapGet("/", () => Results.Json(new { message = "CleanArchitectureTask API." }))
        .ExcludeFromDescription();
}

try
{
    Log.Information("Starting CleanArchitectureTask API");
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}
