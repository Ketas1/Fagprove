using System.Net;
using System.Net.Http.Json;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// Cross-cutting tests for <c>RequireLinkedStaffMiddleware</c> itself,
/// exercised against a couple of representative endpoints rather than
/// duplicated per-controller - see docs/adr/0019-staff-auth0-mapping.md.
/// </summary>
public class RequireLinkedStaffMiddlewareTests
{
    [Fact]
    public async Task An_authenticated_but_unlinked_user_gets_403_from_a_protected_endpoint()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/borrowers");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("StaffNotLinked", problem?.Reason);
    }

    [Fact]
    public async Task A_linked_user_reaches_a_protected_endpoint_normally()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/borrowers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task An_unlinked_user_can_still_reach_the_health_check()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
