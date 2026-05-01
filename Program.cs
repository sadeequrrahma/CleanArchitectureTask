using CleanArchitectureTask.Application;
using CleanArchitectureTask.Common.Models;
using CleanArchitectureTask.Infrastructure;
using CleanArchitectureTask.Infrastructure.Data;
using CleanArchitectureTask.Middleware;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
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
            "REST API. Profile endpoints require **`userId`** query (GUID from login/register **`data.user.id`**)."
    });
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
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = "swagger";
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();

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
