using System.Net.Http.Json;

namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// Creates prerequisite resources via real HTTP calls against an
/// authenticated test client, so integration tests can set up a category,
/// guardian, borrower or piece of equipment without duplicating the request
/// bodies in every test file.
/// </summary>
public static class ApiTestDataBuilder
{
    public static async Task<Guid> CreateCategoryAsync(HttpClient client, string? name = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/equipment-categories", new { Name = name ?? $"Kategori-{Guid.NewGuid()}" });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<IdPayload>())!.Id;
    }

    public static async Task<Guid> CreateGuardianAsync(HttpClient client, string? name = null)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/guardians", new
        {
            Name = name ?? "Test Foresattsen",
            Email = $"{Guid.NewGuid()}@example.no",
            Phone = "12345678",
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<IdPayload>())!.Id;
    }

    public static async Task<Guid> CreateBorrowerAsync(
        HttpClient client, Guid? guardianId = null, DateOnly? dateOfBirth = null)
    {
        Guid resolvedGuardianId = guardianId ?? await CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/borrowers", new
        {
            Name = "Test Barnesen",
            DateOfBirth = dateOfBirth ?? DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)),
            GuardianId = resolvedGuardianId,
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<IdPayload>())!.Id;
    }

    public static async Task<Guid> CreateEquipmentAsync(HttpClient client, Guid? categoryId = null)
    {
        Guid resolvedCategoryId = categoryId ?? await CreateCategoryAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/equipment", new
        {
            Name = "Ski",
            SerialNumber = Guid.NewGuid().ToString(),
            CategoryId = resolvedCategoryId,
            Condition = "New",
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<IdPayload>())!.Id;
    }

    private sealed record IdPayload(Guid Id);
}
