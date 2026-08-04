using Serilog.Context;

namespace AssignmentHub.Api.Middleware;

/// <summary>
/// Stamps every request with a correlation id (reusing an inbound "X-Correlation-Id" header if
/// present, e.g. from a frontend proxy), echoes it back on the response, and pushes it into
/// Serilog's LogContext so every log line for this request can be grepped by that one id.
/// </summary>
public class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing) && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        context.Response.Headers[HeaderName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
