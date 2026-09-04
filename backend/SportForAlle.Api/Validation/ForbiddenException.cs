namespace SportForAlle.Api.Validation;

/// <summary>
/// Thrown when the caller is authenticated but not authorized for this
/// action - currently only "authenticated but not linked to a Staff
/// profile," see Middleware/RequireLinkedStaffMiddleware.cs. Mapped to
/// <c>403</c> by
/// <see cref="SportForAlle.Api.Middleware.ProblemDetailsExceptionHandler"/>.
/// </summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string reason, string detail)
        : base(detail)
    {
        Reason = reason;
    }

    public string Reason { get; }
}
