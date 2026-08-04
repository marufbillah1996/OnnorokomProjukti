using AssignmentHub.Api.Extensions;
using AssignmentHub.Application.Common.Interfaces;
using AssignmentHub.Infrastructure.Persistence;
using AssignmentHub.Infrastructure.Persistence.Seed;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.RateLimiting;

namespace AssignmentHub.IntegrationTests;

/// <summary>
/// Boots the real Api host (<see cref="Program"/>) end to end against a SQLite ":memory:"
/// database instead of the real PostgreSQL one. Implements xUnit's <see cref="IAsyncLifetime"/>
/// so the connection is opened and the schema/seed data are ready before any test in the
/// "Integration" collection runs (see <see cref="IntegrationTestCollection"/>), and torn down
/// once after the whole collection finishes.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    // A SQLite ":memory:" database only exists for as long as its one-and-only connection stays
    // open — the moment that connection closes, the database (schema + data) is gone. Every
    // AppDbContext created by the test host must share THIS connection, so it has to live as a
    // field for the lifetime of the factory rather than a local variable scoped to one method.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Program.cs skips its Postgres-flavored MigrateAsync()/DatabaseSeeder call specifically
        // when the environment is "Testing" — this factory takes over that responsibility itself
        // (see InitializeAsync below) using SQLite-native schema creation instead.
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            // AddJwtAuthentication throws InvalidOperationException if "Jwt:Secret" is missing,
            // and there is no appsettings.Testing.json in this test host — supply the minimum
            // configuration the real pipeline needs directly.
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "integration-test-signing-secret-please-ignore-32chars",
                ["Jwt:Issuer"] = "AssignmentHub",
                ["Jwt:Audience"] = "AssignmentHub.Client",
                ["Cors:AllowedOrigins:0"] = "http://localhost:3000"
            });
        });

        builder.ConfigureServices(services =>
        {
            // ---- Swap the real Npgsql-backed AppDbContext for one pinned to our open SQLite connection ----
            var dbContextDescriptors = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                (d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)) ||
                d.ServiceType == typeof(AppDbContext)).ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            // ---- Neutralize the "auth" rate-limiting policy for the test run ----
            // AddAppRateLimiting (Program.cs) caps /api/auth/login and /api/auth/refresh at 10
            // requests/minute per client IP. Every test in the "Integration" collection shares
            // this one factory/host instance, and TestServer reports the same loopback address
            // for every request, so the many logins performed across LoginEndpointTests,
            // AssignmentEndpointAuthorizationTests and SubmissionWorkflowTests would all share a
            // single fixed window and risk spurious 429 responses unrelated to what each test is
            // actually verifying. Re-registering the policy with an unlimited partitioner keeps
            // real authentication/authorization running end to end without that production-tuned
            // throttle interfering with the test suite.
            var rateLimiterOptionsDescriptors = services.Where(d =>
                d.ServiceType == typeof(IConfigureOptions<RateLimiterOptions>) ||
                d.ServiceType == typeof(IPostConfigureOptions<RateLimiterOptions>)).ToList();

            foreach (var descriptor in rateLimiterOptionsDescriptors)
            {
                services.Remove(descriptor);
            }

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.AddPolicy(RateLimitingExtensions.AuthPolicy, httpContext =>
                    RateLimitPartition.GetNoLimiter(httpContext.Connection.RemoteIpAddress?.ToString() ?? "test-client"));
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Builds the schema straight from the current EF model using SQLite-native DDL. The
        // committed migrations under Infrastructure/Persistence/Migrations contain PostgreSQL
        // (Npgsql) specific column type strings that are not valid against SQLite.
        await dbContext.Database.EnsureCreatedAsync();

        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Provider-agnostic, plain EF Core/LINQ, and already idempotent — reusing it means these
        // integration tests exercise the exact same seed data/path used in production and demos.
        await DatabaseSeeder.SeedAsync(dbContext, passwordHasher);
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
