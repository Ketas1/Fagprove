namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// Minimal shape for reading back the ProblemDetails responses the API
/// returns on 400/404/409/422, see docs/05-api.md. <c>Reason</c> only
/// appears on 409/422 conflicts, see
/// Middleware/ProblemDetailsExceptionHandler.cs.
/// </summary>
public sealed record ProblemPayload(string? Title, string? Detail, string? Reason);
