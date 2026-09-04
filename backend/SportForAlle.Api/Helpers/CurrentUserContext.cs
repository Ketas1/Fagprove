namespace SportForAlle.Api.Helpers;

/// <summary>
/// Per-request identity, populated by
/// <see cref="SportForAlle.Api.Middleware.RequireLinkedStaffMiddleware"/>.
/// Registered scoped, so every service resolved within the same request
/// sees the same instance.
/// </summary>
public class CurrentUserContext
{
    /// <summary>The Auth0 "sub" claim, set for any authenticated request.</summary>
    public string? Auth0Subject { get; set; }

    /// <summary>
    /// The linked <see cref="Models.Staff"/> id, set once
    /// <see cref="SportForAlle.Api.Middleware.RequireLinkedStaffMiddleware"/>
    /// has resolved one. Null for endpoints marked
    /// <see cref="SportForAlle.Api.Middleware.AllowUnlinkedStaffAttribute"/>,
    /// where no link is required.
    /// </summary>
    public Guid? StaffId { get; set; }

    /// <summary>
    /// The linked Staff id, guaranteed present for any service running
    /// behind the middleware. Throws only if that guarantee has been broken
    /// - a bug in the wiring, not something a caller can trigger.
    /// </summary>
    public Guid RequireStaffId() =>
        StaffId ?? throw new InvalidOperationException(
            $"{nameof(CurrentUserContext)}.{nameof(StaffId)} was not populated - " +
            "RequireLinkedStaffMiddleware should have guaranteed this for any endpoint " +
            "not marked [AllowUnlinkedStaff].");
}
