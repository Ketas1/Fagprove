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

    [Fact]
    public async Task Create_allows_the_same_name_under_two_different_parents()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        string childName = $"Diverse-{Guid.NewGuid()}";

        CategoryPayload parentA = (await CreateCategory(client, $"Vintersport-{Guid.NewGuid()}"))!;
        CategoryPayload parentB = (await CreateCategory(client, $"Sykler-{Guid.NewGuid()}"))!;

        HttpResponseMessage firstChild = await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = childName, ParentCategoryId = parentA.Id });
        HttpResponseMessage secondChild = await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = childName, ParentCategoryId = parentB.Id });

        Assert.Equal(HttpStatusCode.Created, firstChild.StatusCode);
        Assert.Equal(HttpStatusCode.Created, secondChild.StatusCode);
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_name_under_the_same_parent()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        CategoryPayload parent = (await CreateCategory(client, $"Vintersport-{Guid.NewGuid()}"))!;
        string childName = $"Ski-{Guid.NewGuid()}";
        await client.PostAsJsonAsync("/api/equipment-categories", new { Name = childName, ParentCategoryId = parent.Id });

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = childName, ParentCategoryId = parent.Id });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("DuplicateCategoryName", problem?.Reason);
    }

    [Fact]
    public async Task Create_rejects_an_unknown_parent_with_404()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = $"Ski-{Guid.NewGuid()}", ParentCategoryId = Guid.NewGuid() });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_changes_the_name()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        CategoryPayload category = (await CreateCategory(client, $"Ski-{Guid.NewGuid()}"))!;
        string newName = $"Slalåmski-{Guid.NewGuid()}";

        HttpResponseMessage response = await client.PutAsJsonAsync(
            $"/api/equipment-categories/{category.Id}", new { Name = newName });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        CategoryPayload? renamed = await response.Content.ReadFromJsonAsync<CategoryPayload>();
        Assert.Equal(newName, renamed?.Name);
    }

    [Fact]
    public async Task Delete_removes_an_empty_category()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        CategoryPayload category = (await CreateCategory(client, $"Ski-{Guid.NewGuid()}"))!;

        HttpResponseMessage response = await client.DeleteAsync($"/api/equipment-categories/{category.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_rejects_a_category_with_a_subcategory()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        CategoryPayload parent = (await CreateCategory(client, $"Vintersport-{Guid.NewGuid()}"))!;
        await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = $"Ski-{Guid.NewGuid()}", ParentCategoryId = parent.Id });

        HttpResponseMessage response = await client.DeleteAsync($"/api/equipment-categories/{parent.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("CategoryHasSubcategories", problem?.Reason);
    }

    private static async Task<CategoryPayload?> CreateCategory(HttpClient client, string name)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/equipment-categories", new { Name = name });
        return await response.Content.ReadFromJsonAsync<CategoryPayload>();
    }

    private sealed record CategoryPayload(Guid Id, string Name, Guid? ParentCategoryId);
}
