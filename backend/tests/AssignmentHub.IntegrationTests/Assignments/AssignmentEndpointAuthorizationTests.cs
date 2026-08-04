using System.Net;
using System.Net.Http.Json;
using AssignmentHub.IntegrationTests;
using FluentAssertions;

namespace AssignmentHub.IntegrationTests.Assignments;

/// <summary>
/// Exercises both flavors of authorization the real pipeline enforces: role-based
/// ([Authorize(Roles=...)]) and resource-based (AssignmentOwnershipRequirement/Handler), plus the
/// assignment-visibility business rule that drafts are never returned to students.
/// </summary>
[Collection("Integration")]
public class AssignmentEndpointAuthorizationTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AssignmentEndpointAuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Captures only what each test needs from AssignmentHub.Application.Assignments.Dtos.AssignmentDto
    // and the PaginatedList<T> envelope it's wrapped in (both camelCase on the wire).
    private sealed record AssignmentSummary(Guid Id, string Title);
    private sealed record PaginatedAssignments(IReadOnlyList<AssignmentSummary> Items);

    [Fact]
    public async Task GetUsers_AsStudent_ReturnsForbidden()
    {
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "student1@assignmenthub.local", "Student@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var response = await client.GetAsync("/api/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetOwnAssignments_AsTeacher1_ReturnsOkContainingAlgebraBasics()
    {
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "teacher1@assignmenthub.local", "Teacher@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var response = await client.GetAsync("/api/assignments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PaginatedAssignments>(JsonDefaults.CaseInsensitive);

        page.Should().NotBeNull();
        page!.Items.Should().Contain(a => a.Title == "Algebra Basics");
    }

    [Fact]
    public async Task UpdateAssignment_AsNonOwnerTeacher_ReturnsForbidden()
    {
        // Teacher1 owns "Algebra Basics" — fetch its id using Teacher1's own token first.
        var teacher1Client = _factory.CreateClient();
        var teacher1Token = await TestAuthHelper.LoginAsync(teacher1Client, "teacher1@assignmenthub.local", "Teacher@123");
        TestAuthHelper.AttachBearerToken(teacher1Client, teacher1Token);

        var listResponse = await teacher1Client.GetAsync("/api/assignments");
        var page = await listResponse.Content.ReadFromJsonAsync<PaginatedAssignments>(JsonDefaults.CaseInsensitive);
        var algebraBasicsId = page!.Items.First(a => a.Title == "Algebra Basics").Id;

        // Teacher2 owns none of Teacher1's assignments — attempting to update this one must be
        // rejected by the resource-based AssignmentOwnershipRequirement, not merely allowed
        // because the caller happens to be *a* Teacher.
        var teacher2Client = _factory.CreateClient();
        var teacher2Token = await TestAuthHelper.LoginAsync(teacher2Client, "teacher2@assignmenthub.local", "Teacher@123");
        TestAuthHelper.AttachBearerToken(teacher2Client, teacher2Token);

        var updateRequest = new
        {
            title = "Hijacked Title",
            description = "An update attempted by a teacher who does not own this assignment.",
            deadlineUtc = DateTime.UtcNow.AddDays(30),
            maxMarks = 100
        };

        var response = await teacher2Client.PutAsJsonAsync($"/api/assignments/{algebraBasicsId}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPublishedAssignments_AsStudent1_ExcludesDraftAssignments()
    {
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "student1@assignmenthub.local", "Student@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var response = await client.GetAsync("/api/student/assignments");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var page = await response.Content.ReadFromJsonAsync<PaginatedAssignments>(JsonDefaults.CaseInsensitive);

        page.Should().NotBeNull();
        // "Essay Draft" is Teacher2's Draft-status assignment on Class9+English — draft
        // assignments must never be visible to students, regardless of class enrollment.
        page!.Items.Should().NotContain(a => a.Title == "Essay Draft");
    }
}
