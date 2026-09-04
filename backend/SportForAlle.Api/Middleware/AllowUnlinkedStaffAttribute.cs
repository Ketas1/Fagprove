namespace SportForAlle.Api.Middleware;

/// <summary>
/// Marks an action (or controller) as reachable by an authenticated user who
/// is not yet linked to a <see cref="Models.Staff"/> profile - the Staff
/// bootstrap endpoints themselves, and the health check. Read by
/// <see cref="RequireLinkedStaffMiddleware"/>, the same way the framework's
/// own <c>[AllowAnonymous]</c> is read by the authorization middleware.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class AllowUnlinkedStaffAttribute : Attribute
{
}
