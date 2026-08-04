using AssignmentHub.Api.Extensions;
using AssignmentHub.Api.Filters;
using AssignmentHub.Api.Middleware;
using AssignmentHub.Application;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Infrastructure;
using AssignmentHub.Infrastructure.Persistence;
using AssignmentHub.Infrastructure.Persistence.Seed;
using AssignmentHub.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ---- Logging (Serilog: console + rolling file, correlation id enriched via middleware) ----
builder.Host.UseSerilog((context, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(AppContext.BaseDirectory, "logs", "assignmenthub-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14);
});

// ---- Services ----
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAppRateLimiting();
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

var app = builder.Build();

// ---- Apply migrations + seed demo data on startup ----
// This is what guarantees "the evaluator can set up the database without manually creating
// tables" (see DocsAndPlan/IMPLEMENTATION_PLAN.md sections 8/13) — docker-compose up alone is enough.
// Skipped under the "Testing" environment: the integration test host swaps in a SQLite
// in-memory database, and the migration files below contain Npgsql-specific column type
// strings that only apply against a real PostgreSQL provider. The test factory builds its
// SQLite schema itself via EnsureCreated() instead (see IntegrationTests/CustomWebApplicationFactory).
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);
}

// ---- Middleware pipeline ----
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger is intentionally enabled in every environment (not gated to Development) — the
// project brief calls for an optional public Swagger/API URL, and this is a recruitment/demo
// project rather than a production service with sensitive internals to hide.
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "AssignmentHub API v1");
});

app.UseCors(CorsServiceExtensions.FrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.UseRateLimiter();

app.MapControllers();
app.MapHub<NotificationsHub>("/hubs/notifications");
app.MapHealthChecks("/health");

app.Run();

/// <summary>Exposed so WebApplicationFactory&lt;Program&gt; in the integration test project can bootstrap this app.</summary>
public partial class Program
{
}
