using System.Net;
using System.Net.Http.Json;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// PUT /api/loans/{id} corrects an open loan. These are the highest-risk paths
/// in the editing work: reassigning equipment has to move both items through
/// the Utstyrstatus arrows, reassigning a borrower has to re-run business rule
/// 2, and a closed loan must be refused outright so the reports keep their
/// basis. See ADR-0025.
/// </summary>
public class LoanCorrectionTests
{
    [Fact]
    public async Task Dates_on_an_open_loan_can_be_corrected()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);

        DateTimeOffset newDue = DateTimeOffset.UtcNow.AddDays(30);
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            StartedAt = DateTimeOffset.UtcNow.AddDays(-1),
            DueDate = newDue,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LoanPayload updated = (await response.Content.ReadFromJsonAsync<LoanPayload>())!;
        Assert.Equal("Active", updated.Status);
    }

    [Fact]
    public async Task Reassigning_equipment_frees_the_previous_item_and_takes_the_new_one()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid originalEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        Guid replacementEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, originalEquipmentId);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = replacementEquipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        EquipmentPayload previous =
            (await client.GetFromJsonAsync<EquipmentPayload>($"/api/equipment/{originalEquipmentId}"))!;
        EquipmentPayload replacement =
            (await client.GetFromJsonAsync<EquipmentPayload>($"/api/equipment/{replacementEquipmentId}"))!;

        Assert.Equal("Available", previous.Status);
        Assert.Equal("OnLoan", replacement.Status);

        // Released, not returned - the condition must be untouched.
        Assert.Equal("New", previous.Condition);
    }

    [Fact]
    public async Task A_returned_loan_cannot_be_corrected()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);
        await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { Condition = "Good" });

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("LoanAlreadyClosed", problem?.Reason);
    }

    [Fact]
    public async Task Reassigning_to_a_banned_borrower_is_refused()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid bannedBorrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);

        HttpResponseMessage banResponse = await client.PostAsJsonAsync(
            $"/api/borrowers/{bannedBorrowerId}/ban", new { Reason = "Mistet utstyr gjentatte ganger." });
        banResponse.EnsureSuccessStatusCode();

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = bannedBorrowerId,
            EquipmentId = equipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("BorrowerBanned", problem?.Reason);
    }

    [Fact]
    public async Task Reassigning_to_equipment_that_is_already_out_is_refused()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid categoryId = await ApiTestDataBuilder.CreateCategoryAsync(client);
        Guid firstBorrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid secondBorrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid firstEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);
        Guid busyEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client, categoryId);

        LoanPayload loan = await RegisterLoanAsync(client, firstBorrowerId, firstEquipmentId);
        await RegisterLoanAsync(client, secondBorrowerId, busyEquipmentId);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = firstBorrowerId,
            EquipmentId = busyEquipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("EquipmentNotAvailable", problem?.Reason);
    }

    [Fact]
    public async Task Keeping_the_same_equipment_does_not_trip_the_availability_rule()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);

        // The item is OnLoan because of this very loan; correcting only the
        // dates must not be refused for that reason.
        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(21),
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task A_due_date_before_the_start_date_is_refused()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            StartedAt = DateTimeOffset.UtcNow,
            DueDate = DateTimeOffset.UtcNow.AddDays(-1),
        });

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    /// <summary>
    /// Regression: PostgreSQL refuses any offset but 0 for a
    /// 'timestamp with time zone', so a client sending a local-offset
    /// timestamp used to crash the update with a 500 instead of saving. The
    /// entity now normalises to UTC, which preserves the instant.
    /// </summary>
    [Fact]
    public async Task A_non_utc_offset_is_accepted_and_stored_as_the_same_instant()
    {
        await using AuthenticatedWebApplicationFactory<Program> factory = new();
        HttpClient client = factory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        LoanPayload loan = await RegisterLoanAsync(client, borrowerId, equipmentId);

        DateTimeOffset osloDueDate = new(2026, 12, 24, 23, 59, 59, TimeSpan.FromHours(2));

        HttpResponseMessage response = await client.PutAsJsonAsync($"/api/loans/{loan.Id}", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            StartedAt = new DateTimeOffset(2026, 12, 1, 0, 0, 0, TimeSpan.FromHours(2)),
            DueDate = osloDueDate,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        LoanDatesPayload stored =
            (await client.GetFromJsonAsync<LoanDatesPayload>($"/api/loans/{loan.Id}"))!;
        Assert.Equal(osloDueDate.ToUniversalTime(), stored.DueDate.ToUniversalTime());
    }

    private static async Task<LoanPayload> RegisterLoanAsync(HttpClient client, Guid borrowerId, Guid equipmentId)
    {
        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<LoanPayload>())!;
    }

    private sealed record LoanPayload(Guid Id, string Status);

    private sealed record LoanDatesPayload(Guid Id, DateTimeOffset StartedAt, DateTimeOffset DueDate);

    private sealed record EquipmentPayload(Guid Id, string Condition, string Status);
}
