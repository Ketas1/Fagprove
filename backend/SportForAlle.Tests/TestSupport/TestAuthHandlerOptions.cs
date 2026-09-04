using Microsoft.AspNetCore.Authentication;

namespace SportForAlle.Tests.TestSupport;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// When true (the default), <see cref="TestAuthHandler"/> ensures a
    /// <c>Staff</c> row linked to its fixed test subject exists before
    /// authenticating, so tests pass RequireLinkedStaffMiddleware without
    /// each one having to set that up itself. Set false to test the
    /// unlinked path deliberately.
    /// </summary>
    public bool SeedLinkedStaff { get; set; } = true;

    public string Subject { get; set; } = TestAuthHandler.DefaultSubject;
}
