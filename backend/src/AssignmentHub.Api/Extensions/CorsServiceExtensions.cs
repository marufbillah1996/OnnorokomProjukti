namespace AssignmentHub.Api.Extensions;

public static class CorsServiceExtensions
{
    public const string FrontendPolicy = "Frontend";

    /// <summary>
    /// Frontend (Next.js) and backend run on different origins in both local dev and Docker
    /// Compose, so an explicit allow-list (from configuration, never a wildcard) is required.
    /// AllowCredentials is needed because the frontend sends the access token cookie.
    /// </summary>
    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:3000"];

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }
}
