using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

public class StaffEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/staff");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_is_reachable_by_an_unlinked_but_authenticated_user()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/staff");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_is_reachable_by_an_unlinked_but_authenticated_user()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/staff", new { Name = "Ola Nordmann" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task LinkMe_links_the_callers_own_subject_to_the_given_staff_profile()
    {
        string subject = $"auth0|{Guid.NewGuid()}";
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: subject);
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/staff", new { Name = "Kari Nordmann" });
        StaffPayload created = (await createResponse.Content.ReadFromJsonAsync<StaffPayload>())!;

        HttpResponseMessage linkResponse = await client.PostAsync($"/api/staff/{created.Id}/link-me", null);

        Assert.Equal(HttpStatusCode.OK, linkResponse.StatusCode);
        StaffPayload? linked = await linkResponse.Content.ReadFromJsonAsync<StaffPayload>();
        Assert.Equal(subject, linked?.Auth0UserId);
    }

    [Fact]
    public async Task LinkMe_returns_404_for_a_staff_profile_that_does_not_exist()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsync($"/api/staff/{Guid.NewGuid()}/link-me", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LinkMe_rejects_a_staff_profile_that_is_already_linked()
    {
        string firstSubject = $"auth0|{Guid.NewGuid()}";
        await using AuthenticatedWebApplicationFactory<Program> firstFactory =
            new(seedLinkedStaff: false, subject: firstSubject);
        HttpClient firstClient = firstFactory.CreateClient();
        HttpResponseMessage createResponse = await firstClient.PostAsJsonAsync("/api/staff", new { Name = "Kari Nordmann" });
        StaffPayload created = (await createResponse.Content.ReadFromJsonAsync<StaffPayload>())!;
        await firstClient.PostAsync($"/api/staff/{created.Id}/link-me", null);

        string secondSubject = $"auth0|{Guid.NewGuid()}";
        await using AuthenticatedWebApplicationFactory<Program> secondFactory =
            new(seedLinkedStaff: false, subject: secondSubject);
        HttpClient secondClient = secondFactory.CreateClient();

        HttpResponseMessage response = await secondClient.PostAsync($"/api/staff/{created.Id}/link-me", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("StaffAlreadyLinked", problem?.Reason);
    }

    [Fact]
    public async Task LinkMe_rejects_an_account_already_linked_to_a_different_staff_profile()
    {
        string subject = $"auth0|{Guid.NewGuid()}";
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: subject);
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage firstCreate = await client.PostAsJsonAsync("/api/staff", new { Name = "Kari Nordmann" });
        StaffPayload first = (await firstCreate.Content.ReadFromJsonAsync<StaffPayload>())!;
        await client.PostAsync($"/api/staff/{first.Id}/link-me", null);

        HttpResponseMessage secondCreate = await client.PostAsJsonAsync("/api/staff", new { Name = "Ola Nordmann" });
        StaffPayload second = (await secondCreate.Content.ReadFromJsonAsync<StaffPayload>())!;

        HttpResponseMessage response = await client.PostAsync($"/api/staff/{second.Id}/link-me", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("Auth0AccountAlreadyLinked", problem?.Reason);
    }

    [Fact]
    public async Task GetMe_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/staff/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_returns_404_for_an_authenticated_but_unlinked_user()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: $"auth0|{Guid.NewGuid()}");
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/staff/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_returns_the_callers_own_profile_once_linked()
    {
        string subject = $"auth0|{Guid.NewGuid()}";
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory =
            new(seedLinkedStaff: false, subject: subject);
        HttpClient client = authenticatedFactory.CreateClient();
        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/staff", new { Name = "Kari Nordmann" });
        StaffPayload created = (await createResponse.Content.ReadFromJsonAsync<StaffPayload>())!;
        await client.PostAsync($"/api/staff/{created.Id}/link-me", null);

        HttpResponseMessage response = await client.GetAsync("/api/staff/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        StaffPayload? me = await response.Content.ReadFromJsonAsync<StaffPayload>();
        Assert.Equal(created.Id, me?.Id);
        Assert.Equal(subject, me?.Auth0UserId);
    }

    private sealed record StaffPayload(Guid Id, string Name, string? Auth0UserId);
}
