using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

public class GuardiansEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/guardians");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_returns_a_created_guardian()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        List<GuardianPayload>? guardians = await client.GetFromJsonAsync<List<GuardianPayload>>("/api/guardians");

        Assert.Contains(guardians!, guardian => guardian.Id == guardianId);
    }

    [Fact]
    public async Task Create_then_get_returns_the_new_guardian()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/guardians", new
        {
            Name = "Kari Nordmann",
            Email = $"{Guid.NewGuid()}@example.no",
            Phone = "12345678",
        });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        GuardianPayload created = (await createResponse.Content.ReadFromJsonAsync<GuardianPayload>())!;

        GuardianPayload? fetched = await client.GetFromJsonAsync<GuardianPayload>($"/api/guardians/{created.Id}");

        Assert.Equal("Kari Nordmann", fetched?.Name);
    }

    [Fact]
    public async Task Update_changes_the_guardians_details()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client, "Ola Nordmann");

        HttpResponseMessage updateResponse = await client.PutAsJsonAsync($"/api/guardians/{guardianId}", new
        {
            Name = "Ola Nordmann Jr.",
            Email = $"{Guid.NewGuid()}@example.no",
            Phone = "87654321",
        });

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        GuardianPayload? updated = await updateResponse.Content.ReadFromJsonAsync<GuardianPayload>();
        Assert.Equal("Ola Nordmann Jr.", updated?.Name);
    }

    [Fact]
    public async Task GetById_returns_404_for_a_guardian_that_does_not_exist()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/api/guardians/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_populates_CreatedByStaffId_with_a_real_staff_member()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/guardians", new
        {
            Name = "Per Hansen",
            Email = $"{Guid.NewGuid()}@example.no",
            Phone = "12345678",
        });
        GuardianPayload created = (await createResponse.Content.ReadFromJsonAsync<GuardianPayload>())!;

        Assert.NotNull(created.CreatedByStaffId);

        List<StaffPayload>? staffMembers = await client.GetFromJsonAsync<List<StaffPayload>>("/api/staff");
        Assert.Contains(staffMembers!, staff => staff.Id == created.CreatedByStaffId);
    }

    private sealed record GuardianPayload(Guid Id, string Name, string Email, string Phone, Guid? CreatedByStaffId);

    private sealed record StaffPayload(Guid Id, string Name, string? Auth0UserId);
}
