using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

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
    public async Task Health_endpoint_returns_200()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Health_endpoint_reports_a_status()
    {
        HttpClient client = factory.CreateClient();

        HealthPayload? payload = await client.GetFromJsonAsync<HealthPayload>("/api/health");

        Assert.NotNull(payload);
        Assert.Equal("ok", payload.Status);
        Assert.Contains(payload.Database, new[] { "up", "down" });
    }

    private sealed record HealthPayload(string Status, string Database);
}
