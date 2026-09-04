using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

public class EquipmentCategoriesEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/equipment-categories");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_then_list_returns_the_new_category()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        string name = $"Kategori-{Guid.NewGuid()}";

        HttpResponseMessage createResponse =
            await client.PostAsJsonAsync("/api/equipment-categories", new { Name = name });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        List<CategoryPayload>? categories =
            await client.GetFromJsonAsync<List<CategoryPayload>>("/api/equipment-categories");

        Assert.Contains(categories!, category => category.Name == name);
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_name_with_409_and_a_reason()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        string name = $"Kategori-{Guid.NewGuid()}";
        await client.PostAsJsonAsync("/api/equipment-categories", new { Name = name });

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/equipment-categories", new { Name = name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("DuplicateCategoryName", problem?.Reason);
    }

    private sealed record CategoryPayload(Guid Id, string Name);
}
