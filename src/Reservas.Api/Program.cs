using Reservas.Api.Modules;
using Reservas.Api.Modules.Platform;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddPlatformModule(builder.Configuration)
    .AddApplicationModules(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapHealthChecks("/health/live").DisableRateLimiting();
app.MapControllers().RequireRateLimiting(PlatformPolicies.ApiRateLimit);
app.MapApplicationModules();

app.Run();

/// <summary>
/// Exposes the application entry point to the integration-test host.
/// </summary>
public partial class Program;
