using System.Net;
using System.Net.Http.Json;
using AssignmentHub.IntegrationTests;
using FluentAssertions;

namespace AssignmentHub.IntegrationTests.Auth;

/// <summary>
/// Drives the real /api/auth/login endpoint (and one protected endpoint) through the full
/// ASP.NET Core pipeline — no mocking of auth internals.
/// </summary>
[Collection("Integration")]
public class LoginEndpointTests
{
    private readonly CustomWebApplicationFactory _factory;

    public LoginEndpointTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Mirrors AssignmentHub.Application.Auth.Dtos.LoginResponse's camelCase JSON shape, capturing
    // only the properties this test asserts on.
    private sealed record LoginResponseBody(string AccessToken, string Role);

    [Fact]
    public async Task Login_WithValidAdminCredentials_ReturnsOkWithAccessTokenAndAdminRole()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@assignmenthub.local",
            password = "Admin@123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseBody>(JsonDefaults.CaseInsensitive);

        body.Should().NotBeNull();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@assignmenthub.local",
            password = "definitely-the-wrong-password"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUsers_WithoutAuthorizationHeader_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
