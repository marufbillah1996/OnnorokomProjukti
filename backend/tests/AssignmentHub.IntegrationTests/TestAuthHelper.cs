using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AssignmentHub.IntegrationTests;

/// <summary>
/// Shared JSON options for deserializing black-box HTTP responses in these tests: ASP.NET Core's
/// default output naming policy is camelCase, but the local record types used to capture just the
/// properties each test needs are declared with ordinary PascalCase C# property names.
/// </summary>
internal static class JsonDefaults
{
    public static readonly JsonSerializerOptions CaseInsensitive = new(JsonSerializerDefaults.Web);
}

/// <summary>
/// Drives the real /api/auth/login endpoint so tests can obtain a genuine, fully-signed JWT for
/// one of the seeded demo accounts (see DatabaseSeeder) and attach it to subsequent requests,
/// rather than fabricating a token or bypassing authentication.
/// </summary>
public static class TestAuthHelper
{
    // Mirrors the JSON shape of AssignmentHub.Application.Auth.Dtos.LoginResponse as it appears
    // on the wire (camelCase), capturing only the property every caller of LoginAsync needs.
    private sealed record LoginResponseBody(
        string AccessToken,
        DateTime AccessTokenExpiresAtUtc,
        string RefreshToken,
        DateTime RefreshTokenExpiresAtUtc,
        Guid UserId,
        string Name,
        string Email,
        string Role);

    public static async Task<string> LoginAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>(JsonDefaults.CaseInsensitive);

        return body?.AccessToken
            ?? throw new InvalidOperationException("Login succeeded but the response body did not contain an accessToken.");
    }

    public static void AttachBearerToken(HttpClient client, string accessToken)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
}
