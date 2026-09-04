using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

public class BorrowersEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/borrowers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_returns_a_created_borrower()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);

        List<BorrowerPayload>? borrowers = await client.GetFromJsonAsync<List<BorrowerPayload>>("/api/borrowers");

        Assert.Contains(borrowers!, borrower => borrower.Id == borrowerId);
    }

    [Fact]
    public async Task Create_with_an_inline_new_guardian_creates_both()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Lise Barnesen",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-8)),
            NewGuardian = new { Name = "Per Barnesen", Email = $"{Guid.NewGuid()}@example.no", Phone = "11223344" },
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        BorrowerPayload? created = await response.Content.ReadFromJsonAsync<BorrowerPayload>();
        Assert.Equal("Per Barnesen", created?.GuardianName);
    }

    [Fact]
    public async Task Create_with_an_existing_guardian_links_it_instead_of_creating_a_new_one()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client, "Delt Foresatt");

        HttpResponseMessage firstChild = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Barn En",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-6)),
            GuardianId = guardianId,
        });
        HttpResponseMessage secondChild = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Barn To",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-9)),
            GuardianId = guardianId,
        });

        Assert.Equal(HttpStatusCode.Created, firstChild.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondChild.StatusCode);
        BorrowerPayload? first = await firstChild.Content.ReadFromJsonAsync<BorrowerPayload>();
        BorrowerPayload? second = await secondChild.Content.ReadFromJsonAsync<BorrowerPayload>();
        Assert.Equal(first?.GuardianId, second?.GuardianId);
    }

    [Fact]
    public async Task Create_rejects_both_guardianId_and_newGuardian_given_together()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Barn Med To Foresatte-forsøk",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-6)),
            GuardianId = guardianId,
            NewGuardian = new { Name = "Ekstra Foresatt", Email = $"{Guid.NewGuid()}@example.no", Phone = "11223344" },
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_rejects_neither_guardianId_nor_newGuardian_given()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Barn Uten Foresatt",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-6)),
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_rejects_a_borrower_younger_than_three_with_422_and_a_reason()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "For Ung",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-1)),
            GuardianId = guardianId,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("BorrowerOutsideAgeRange", problem?.Reason);
    }

    [Fact]
    public async Task Create_rejects_a_borrower_older_than_eighteen()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "For Gammel",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-19)),
            GuardianId = guardianId,
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private sealed record BorrowerPayload(Guid Id, string Name, Guid GuardianId, string GuardianName);
}
