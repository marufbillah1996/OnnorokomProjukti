using System.Net;
using System.Net.Http.Json;
using AssignmentHub.IntegrationTests;
using FluentAssertions;

namespace AssignmentHub.IntegrationTests.Submissions;

/// <summary>
/// Drives the student submit/update and teacher grade endpoints end to end against the seeded
/// demo data, asserting the domain business rules (duplicate submissions, deadlines, marks range)
/// surface as the correct HTTP status codes through the real exception-handling middleware.
/// </summary>
[Collection("Integration")]
public class SubmissionWorkflowTests
{
    private readonly CustomWebApplicationFactory _factory;

    public SubmissionWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private sealed record AssignmentSummary(Guid Id, string Title);
    private sealed record PaginatedAssignments(IReadOnlyList<AssignmentSummary> Items);

    private sealed record SubmissionSummary(Guid Id, string AssignmentTitle, string Status, int? MarksAwarded);
    private sealed record PaginatedSubmissions(IReadOnlyList<SubmissionSummary> Items);

    [Fact]
    public async Task Submit_ToAssignmentWithExistingSubmission_ReturnsConflict()
    {
        // Student1 already has a Pending submission on "Algebra Basics" per seed data — POSTing
        // to the submit endpoint again must be rejected as a duplicate, not silently overwrite it.
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "student1@assignmenthub.local", "Student@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var assignmentsResponse = await client.GetAsync("/api/student/assignments");
        var assignmentsPage = await assignmentsResponse.Content.ReadFromJsonAsync<PaginatedAssignments>(JsonDefaults.CaseInsensitive);
        var algebraBasicsId = assignmentsPage!.Items.First(a => a.Title == "Algebra Basics").Id;

        var response = await client.PostAsJsonAsync(
            $"/api/student/assignments/{algebraBasicsId}/submit",
            new { content = "A second attempt at an assignment I've already submitted." });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task UpdateSubmission_ForAssignmentPastDeadline_ReturnsConflict()
    {
        // Student1's submission on "Quadratic Equations" is Returned (student-modifiable per
        // Submission.CanBeModifiedByStudent), but that assignment's deadline is already in the
        // past — the deadline rule must still block the update.
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "student1@assignmenthub.local", "Student@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var submissionsResponse = await client.GetAsync("/api/student/submissions");
        var submissionsPage = await submissionsResponse.Content.ReadFromJsonAsync<PaginatedSubmissions>(JsonDefaults.CaseInsensitive);
        var quadraticSubmissionId = submissionsPage!.Items.First(s => s.AssignmentTitle == "Quadratic Equations").Id;

        var response = await client.PutAsJsonAsync(
            $"/api/student/submissions/{quadraticSubmissionId}",
            new { content = "Trying to resubmit after the deadline has already passed." });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GradeSubmission_MarksExceedingMaxThenValidMarks_ReturnsBadRequestThenOkGraded()
    {
        var client = _factory.CreateClient();
        var token = await TestAuthHelper.LoginAsync(client, "teacher1@assignmenthub.local", "Teacher@123");
        TestAuthHelper.AttachBearerToken(client, token);

        var assignmentsResponse = await client.GetAsync("/api/assignments");
        var assignmentsPage = await assignmentsResponse.Content.ReadFromJsonAsync<PaginatedAssignments>(JsonDefaults.CaseInsensitive);
        var algebraBasicsId = assignmentsPage!.Items.First(a => a.Title == "Algebra Basics").Id;

        var submissionsResponse = await client.GetAsync($"/api/assignments/{algebraBasicsId}/submissions");
        var submissionsPage = await submissionsResponse.Content.ReadFromJsonAsync<PaginatedSubmissions>(JsonDefaults.CaseInsensitive);
        // Student1's submission is the only Pending one on "Algebra Basics" — Student2's is
        // already Graded per seed data.
        var student1SubmissionId = submissionsPage!.Items.First(s => s.Status == "Pending").Id;

        // "Algebra Basics" has MaxMarks = 100 (seed data) — 150 is out of range.
        var invalidResponse = await client.PutAsJsonAsync(
            $"/api/submissions/{student1SubmissionId}/grade",
            new { marksAwarded = 150, feedback = "This grade should never be persisted." });

        invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var validResponse = await client.PutAsJsonAsync(
            $"/api/submissions/{student1SubmissionId}/grade",
            new { marksAwarded = 90, feedback = "Well done." });

        validResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var graded = await validResponse.Content.ReadFromJsonAsync<SubmissionSummary>(JsonDefaults.CaseInsensitive);

        graded.Should().NotBeNull();
        graded!.Status.Should().Be("Graded");
        graded.MarksAwarded.Should().Be(90);
    }
}
