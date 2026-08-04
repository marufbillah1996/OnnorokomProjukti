using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace AssignmentHub.Api.Extensions;

public static class RateLimitingExtensions
{
    public const string AuthPolicy = "auth";

    /// <summary>
    /// .NET 8's built-in rate limiter (no third-party package needed) — blunts brute-force
    /// attempts against login/refresh: 10 requests per minute per client IP, no queueing
    /// (excess requests get an immediate 429 rather than piling up).
    /// </summary>
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }
}
