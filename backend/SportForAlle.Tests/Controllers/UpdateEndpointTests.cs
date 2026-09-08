using System.Net;
using System.Net.Http.Json;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// Covers the editing surface added so staff can correct data entered wrongly:
/// staff contact details, a date of birth, and equipment serial number and
/// condition. The failure paths matter more than the happy ones - a duplicate
/// serial number must be refused, and equipment status must stay out of reach
/// of the edit form.
/// </summary>
public class UpdateEndpointTests
{
    [Fact]
    public async Task Staff_contact_details_can_be_edited()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        HttpResponseMessage created = await client.PostAsJsonAsync("/api/staff", new { Name = "Kari Ansatt" });
        StaffPayload staff = (await created.Content.ReadFromJsonAsync<StaffPayload>())!;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/staff/{staff.Id}", new
        {
            Name = "Kari Ansatt",
            JobTitle = "Butikkleder",
            Email = "kari@sportforalle.no",
            Phone = "99887766",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        StaffPayload updated = (await response.Content.ReadFromJsonAsync<StaffPayload>())!;
        Assert.Equal("Butikkleder", updated.JobTitle);
        Assert.Equal("kari@sportforalle.no", updated.Email);
        Assert.Equal("99887766", updated.Phone);
    }

    [Fact]
    public async Task Staff_optional_fields_clear_to_null_when_blanked()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        HttpResponseMessage created = await client.PostAsJsonAsync("/api/staff", new
        {
            Name = "Ola Ansatt",
            JobTitle = "Butikkmedarbeider",
            Phone = "11223344",
        });
        StaffPayload staff = (await created.Content.ReadFromJsonAsync<StaffPayload>())!;
        Assert.Equal("Butikkmedarbeider", staff.JobTitle);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/staff/{staff.Id}", new
        {
            Name = "Ola Ansatt",
            JobTitle = "   ",
            Phone = (string?)null,
        });

        StaffPayload updated = (await response.Content.ReadFromJsonAsync<StaffPayload>())!;
        Assert.Null(updated.JobTitle);
        Assert.Null(updated.Phone);
    }

    [Fact]
    public async Task Staff_rejects_a_malformed_email()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        HttpResponseMessage created = await client.PostAsJsonAsync("/api/staff", new { Name = "Per Ansatt" });
        StaffPayload staff = (await created.Content.ReadFromJsonAsync<StaffPayload>())!;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/staff/{staff.Id}", new
        {
            Name = "Per Ansatt",
            Email = "ikke-en-epost",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Borrower_date_of_birth_can_be_corrected()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(
            client, dateOfBirth: new DateOnly(2015, 5, 5));
        DateOnly corrected = new(2016, 6, 6);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/borrowers/{borrowerId}", new
        {
            Name = "Test Barnesen",
            DateOfBirth = corrected,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        BorrowerPayload updated = (await response.Content.ReadFromJsonAsync<BorrowerPayload>())!;
        Assert.Equal(corrected, updated.DateOfBirth);
    }

    [Fact]
    public async Task Borrower_rejects_a_date_of_birth_in_the_future()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/borrowers/{borrowerId}", new
        {
            Name = "Test Barnesen",
            DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
        });

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Guardian_contact_details_can_be_edited()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/guardians/{guardianId}", new
        {
            Name = "Nytt Navn",
            Email = "nytt@example.no",
            Phone = "55443322",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        GuardianPayload updated = (await response.Content.ReadFromJsonAsync<GuardianPayload>())!;
        Assert.Equal("Nytt Navn", updated.Name);
        Assert.Equal("nytt@example.no", updated.Email);
        Assert.Equal("55443322", updated.Phone);
    }

    [Fact]
    public async Task Equipment_serial_number_and_condition_can_be_edited()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        string newSerial = Guid.NewGuid().ToString();

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/equipment/{equipmentId}", new
        {
            Name = "Ski 150cm",
            CategoryId = categoryId,
            SerialNumber = newSerial,
            Condition = "Worn",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        EquipmentPayload updated = (await response.Content.ReadFromJsonAsync<EquipmentPayload>())!;
        Assert.Equal(newSerial, updated.SerialNumber);
        Assert.Equal("Worn", updated.Condition);

        // The edit form must not be able to move status - that belongs to the
        // Utstyrstatus state machine in docs/03-domenemodell.md.
        Assert.Equal("Available", updated.Status);
    }

    [Fact]
    public async Task Equipment_refuses_a_serial_number_already_used_by_another_item()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        Guid firstId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        Guid secondId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        EquipmentPayload first = (await client.GetFromJsonAsync<EquipmentPayload>($"/api/equipment/{firstId}"))!;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/equipment/{secondId}", new
        {
            Name = "Ski",
            CategoryId = categoryId,
            SerialNumber = first.SerialNumber,
            Condition = "Good",
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("DuplicateSerialNumber", problem?.Reason);
    }

    [Fact]
    public async Task Equipment_keeps_its_own_serial_number_on_an_unrelated_edit()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        EquipmentPayload existing = (await client.GetFromJsonAsync<EquipmentPayload>($"/api/equipment/{equipmentId}"))!;

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/equipment/{equipmentId}", new
        {
            Name = "Nytt navn",
            CategoryId = categoryId,
            SerialNumber = existing.SerialNumber,
            Condition = "Good",
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private sealed record StaffPayload(Guid Id, string Name, string? JobTitle, string? Email, string? Phone);

    private sealed record BorrowerPayload(Guid Id, string Name, DateOnly DateOfBirth);

    private sealed record GuardianPayload(Guid Id, string Name, string Email, string Phone);

    private sealed record EquipmentPayload(
        Guid Id, string Name, string SerialNumber, string Condition, string Status);
}
