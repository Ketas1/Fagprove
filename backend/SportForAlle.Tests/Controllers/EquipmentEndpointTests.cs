using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

public class EquipmentEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/equipment");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_returns_the_new_equipment_as_available()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);

        HttpResponseMessage createResponse = await client.PostAsJsonAsync("/api/equipment", new
        {
            Name = "Slalåmski",
            SerialNumber = Guid.NewGuid().ToString(),
            CategoryId = categoryId,
            Condition = "Good",
        });

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        EquipmentPayload? created = await createResponse.Content.ReadFromJsonAsync<EquipmentPayload>();
        Assert.Equal("Available", created?.Status);
    }

    [Fact]
    public async Task Create_rejects_a_duplicate_serial_number_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        string serialNumber = Guid.NewGuid().ToString();
        await client.PostAsJsonAsync("/api/equipment", new
        {
            Name = "Ski A",
            SerialNumber = serialNumber,
            CategoryId = categoryId,
            Condition = "New",
        });

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/equipment", new
        {
            Name = "Ski B",
            SerialNumber = serialNumber,
            CategoryId = categoryId,
            Condition = "New",
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("DuplicateSerialNumber", problem?.Reason);
    }

    [Fact]
    public async Task Create_returns_404_for_a_category_that_does_not_exist()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/equipment", new
        {
            Name = "Ski",
            SerialNumber = Guid.NewGuid().ToString(),
            CategoryId = Guid.NewGuid(),
            Condition = "New",
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_removes_equipment_with_no_loan_history()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);

        HttpResponseMessage response = await client.DeleteAsync($"/api/equipment/{equipmentId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_rejects_equipment_with_loan_history_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        HttpResponseMessage response = await client.DeleteAsync($"/api/equipment/{equipmentId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("EquipmentHasLoanHistory", problem?.Reason);
    }

    private sealed record EquipmentPayload(Guid Id, string Name, string Status);
}
