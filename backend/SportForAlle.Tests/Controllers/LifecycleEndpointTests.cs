using System.Net;
using System.Net.Http.Json;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// Deletion, archiving and anonymisation. The refusals are the point: a
/// borrower or guardian with history must never be hard-deleted, because the
/// loan rows behind the municipality reports would go with them. Those are
/// archived and eventually anonymised instead - see ADR-0026 and
/// docs/09-lover-og-regler.md.
/// </summary>
public class LifecycleEndpointTests
{
    [Fact]
    public async Task A_borrower_with_no_history_can_be_deleted()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);

        HttpResponseMessage response = await client.DeleteAsync($"/api/borrowers/{borrowerId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        HttpResponseMessage lookup = await client.GetAsync($"/api/borrowers/{borrowerId}");
        Assert.Equal(HttpStatusCode.NotFound, lookup.StatusCode);
    }

    [Fact]
    public async Task A_borrower_with_a_loan_cannot_be_deleted()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        HttpResponseMessage response = await client.DeleteAsync($"/api/borrowers/{borrowerId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("BorrowerHasHistory", problem?.Reason);
    }

    [Fact]
    public async Task A_borrower_can_be_archived_and_restored()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);

        HttpResponseMessage archived = await client.PostAsync($"/api/borrowers/{borrowerId}/archive", null);
        Assert.Equal(HttpStatusCode.OK, archived.StatusCode);
        BorrowerPayload afterArchive = (await archived.Content.ReadFromJsonAsync<BorrowerPayload>())!;
        Assert.NotNull(afterArchive.ArchivedAt);

        HttpResponseMessage restored = await client.DeleteAsync($"/api/borrowers/{borrowerId}/archive");
        Assert.Equal(HttpStatusCode.OK, restored.StatusCode);
        BorrowerPayload afterRestore = (await restored.Content.ReadFromJsonAsync<BorrowerPayload>())!;
        Assert.Null(afterRestore.ArchivedAt);
    }

    [Fact]
    public async Task Anonymising_strips_the_name_but_keeps_the_loan_countable()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        HttpResponseMessage response = await client.PostAsync($"/api/borrowers/{borrowerId}/anonymise", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        BorrowerPayload anonymised = (await response.Content.ReadFromJsonAsync<BorrowerPayload>())!;
        Assert.Equal("Anonymisert låntaker", anonymised.Name);
        Assert.NotNull(anonymised.AnonymisedAt);
        Assert.NotNull(anonymised.ArchivedAt);

        // The date of birth survives, so the age-group report still buckets
        // this loan correctly - that is the whole reason for anonymising
        // rather than deleting.
        Assert.NotEqual(default, anonymised.DateOfBirth);

        List<LoanPayload>? loans = await client.GetFromJsonAsync<List<LoanPayload>>("/api/loans");
        Assert.Contains(loans!, loan => loan.BorrowerId == borrowerId);
    }

    [Fact]
    public async Task Anonymising_deletes_the_free_text_notes()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        await client.PostAsJsonAsync($"/api/borrowers/{borrowerId}/notes", new { Text = "Ringte hjem, ingen svar." });

        await client.PostAsync($"/api/borrowers/{borrowerId}/anonymise", null);

        List<NotePayload>? notes =
            await client.GetFromJsonAsync<List<NotePayload>>($"/api/borrowers/{borrowerId}/notes");
        Assert.Empty(notes!);
    }

    [Fact]
    public async Task An_anonymised_borrower_cannot_be_restored()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        await client.PostAsync($"/api/borrowers/{borrowerId}/anonymise", null);

        HttpResponseMessage response = await client.DeleteAsync($"/api/borrowers/{borrowerId}/archive");

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task A_guardian_with_a_registered_child_cannot_be_deleted()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);
        await ApiTestDataBuilder.CreateBorrowerAsync(client, guardianId);

        HttpResponseMessage response = await client.DeleteAsync($"/api/guardians/{guardianId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("GuardianHasBorrowers", problem?.Reason);
    }

    [Fact]
    public async Task A_guardian_with_no_children_can_be_deleted()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.DeleteAsync($"/api/guardians/{guardianId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Anonymising_a_guardian_strips_the_contact_details()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid guardianId = await ApiTestDataBuilder.CreateGuardianAsync(client);

        HttpResponseMessage response = await client.PostAsync($"/api/guardians/{guardianId}/anonymise", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        GuardianPayload anonymised = (await response.Content.ReadFromJsonAsync<GuardianPayload>())!;
        Assert.Equal("Anonymisert foresatt", anonymised.Name);
        Assert.DoesNotContain("@example.no", anonymised.Email);
        Assert.NotNull(anonymised.AnonymisedAt);
    }

    [Fact]
    public async Task A_staff_profile_can_be_deleted()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        HttpResponseMessage created = await client.PostAsJsonAsync("/api/staff", new { Name = "Midlertidig Ansatt" });
        StaffPayload staff = (await created.Content.ReadFromJsonAsync<StaffPayload>())!;

        HttpResponseMessage response = await client.DeleteAsync($"/api/staff/{staff.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Deleting_your_own_staff_profile_is_refused()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        StaffPayload me = (await client.GetFromJsonAsync<StaffPayload>("/api/staff/me"))!;

        HttpResponseMessage response = await client.DeleteAsync($"/api/staff/{me.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("CannotDeleteOwnStaffProfile", problem?.Reason);
    }

    private sealed record BorrowerPayload(
        Guid Id,
        string Name,
        DateOnly DateOfBirth,
        DateTimeOffset? ArchivedAt,
        DateTimeOffset? AnonymisedAt);

    private sealed record GuardianPayload(
        Guid Id, string Name, string Email, string Phone, DateTimeOffset? AnonymisedAt);

    private sealed record LoanPayload(Guid Id, Guid BorrowerId);

    private sealed record NotePayload(Guid Id, string Text);

    private sealed record StaffPayload(Guid Id, string Name);
}
