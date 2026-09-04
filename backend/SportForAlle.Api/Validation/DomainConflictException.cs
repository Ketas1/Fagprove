namespace SportForAlle.Api.Validation;

/// <summary>
/// Thrown when a request is well-formed but violates a business rule - a
/// blocked loan, unavailable equipment, a borrower outside the allowed age
/// range. Mapped to <see cref="StatusCode"/> (default <c>409</c>) by
/// <see cref="SportForAlle.Api.Middleware.ProblemDetailsExceptionHandler"/>,
/// with <see cref="Reason"/> exposed as the machine-readable `reason` field
/// documented in docs/05-api.md.
/// </summary>
public class DomainConflictException : Exception
{
    public DomainConflictException(string reason, string detail, int statusCode = StatusCodes.Status409Conflict)
        : base(detail)
    {
        Reason = reason;
        StatusCode = statusCode;
    }

    public string Reason { get; }

    public int StatusCode { get; }
}
