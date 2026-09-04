using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// The <c>BorrowerBanned</c> and <c>BorrowerHasOverdueLoan</c> 409 reasons
/// are not exercised here: there is no ban endpoint yet (out of scope, see
/// docs/05-api.md), and reaching an overdue loan through the real API would
/// need a due date in the past, which <see cref="SportForAlle.Api.Models.Loan"/>'s
/// constructor rejects. Both are covered at the unit level instead, see
/// SportForAlle.Tests/Services/Rules/LoanRulesTests.cs.
/// </summary>
public class LoansEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetAll_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/loans");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_returns_a_registered_loan()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        LoanPayload registered = (await registerResponse.Content.ReadFromJsonAsync<LoanPayload>())!;

        List<LoanPayload>? loans = await client.GetFromJsonAsync<List<LoanPayload>>("/api/loans");

        Assert.Contains(loans!, loan => loan.Id == registered.Id);
    }

    [Fact]
    public async Task Register_marks_the_equipment_on_loan()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        EquipmentStatusPayload? equipment =
            await client.GetFromJsonAsync<EquipmentStatusPayload>($"/api/equipment/{equipmentId}");
        Assert.Equal("OnLoan", equipment?.Status);
    }

    [Fact]
    public async Task Register_rejects_equipment_already_on_loan_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        Guid firstBorrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid secondBorrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = firstBorrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = secondBorrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("EquipmentNotAvailable", problem?.Reason);
    }

    [Fact]
    public async Task Register_allows_a_second_loan_for_a_borrower_whose_first_loan_is_still_active()
    {
        // Exercises the "open loans" query (Services/LoanService.cs,
        // RegisterAsync) against a real, non-empty result set - a borrower
        // with one open loan is a legitimate everyday case (borrowing two
        // items at once), not just a blocked-loan edge case, and the query
        // filters on an array via .Contains(), which needs to actually
        // execute against Postgres to prove it translates.
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid firstEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        Guid secondEquipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage firstLoan = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = firstEquipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        Assert.Equal(HttpStatusCode.Created, firstLoan.StatusCode);

        HttpResponseMessage secondLoan = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = secondEquipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Created, secondLoan.StatusCode);
    }

    [Fact]
    public async Task Register_returns_404_for_a_borrower_that_does_not_exist()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = Guid.NewGuid(),
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Return_on_time_frees_the_equipment_again()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        LoanPayload loan = (await registerResponse.Content.ReadFromJsonAsync<LoanPayload>())!;

        HttpResponseMessage returnResponse =
            await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { Condition = "Good" });

        Assert.Equal(HttpStatusCode.OK, returnResponse.StatusCode);
        LoanPayload? returned = await returnResponse.Content.ReadFromJsonAsync<LoanPayload>();
        Assert.Equal("Returned", returned?.Status);
        Assert.Equal(0, returned?.DaysLate);

        EquipmentStatusPayload? equipment =
            await client.GetFromJsonAsync<EquipmentStatusPayload>($"/api/equipment/{equipmentId}");
        Assert.Equal("Available", equipment?.Status);
    }

    [Fact]
    public async Task Return_a_damaged_item_takes_the_equipment_out_of_service()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        LoanPayload loan = (await registerResponse.Content.ReadFromJsonAsync<LoanPayload>())!;

        await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { Condition = "Damaged" });

        EquipmentStatusPayload? equipment =
            await client.GetFromJsonAsync<EquipmentStatusPayload>($"/api/equipment/{equipmentId}");
        Assert.Equal("OutOfService", equipment?.Status);
    }

    [Fact]
    public async Task Return_rejects_a_loan_that_is_already_returned_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        LoanPayload loan = (await registerResponse.Content.ReadFromJsonAsync<LoanPayload>())!;
        await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { Condition = "Good" });

        HttpResponseMessage response =
            await client.PostAsJsonAsync($"/api/loans/{loan.Id}/return", new { Condition = "Good" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("LoanAlreadyClosed", problem?.Reason);
    }

    private sealed record LoanPayload(Guid Id, string Status, int? DaysLate);

    private sealed record EquipmentStatusPayload(Guid Id, string Status);
}
