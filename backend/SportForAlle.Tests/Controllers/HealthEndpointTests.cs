using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// Starts the real application in memory. Deliberately does not require a
/// database: the endpoint reports whether the database is reachable, so the
/// test proves the API boots and routes correctly either way.
/// </summary>
public class HealthEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task Health_endpoint_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/health");

        // Every endpoint requires authentication by default - see
        // docs/06-autentisering.md, "Beskyttelse av backend". This is one of
        // the highest-value tests in the project per docs/07-testing.md.
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_reports_a_status_once_authenticated()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HealthPayload? payload = await client.GetFromJsonAsync<HealthPayload>("/api/health");

        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
        Assert.Contains(payload.Database, new[] { "up", "down" });
    }

    private sealed record HealthPayload(string Status, string Database);
}
