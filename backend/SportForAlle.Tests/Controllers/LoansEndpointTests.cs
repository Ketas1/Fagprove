using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Services;
using SportForAlle.Api.Validation;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// <c>BorrowerHasOverdueLoan</c> is not exercised here: reaching an overdue loan
/// through the real API needs a due date in the past, which
/// <see cref="SportForAlle.Api.Models.Loan"/>'s constructor rejects outright.
/// It is covered at the unit level instead
/// (SportForAlle.Tests/Services/Rules/LoanRulesTests.cs) and, for the write-time
/// materialisation itself, in Controllers/OverdueLoanRefreshTests.cs.
/// <c>BorrowerBanned</c> is exercised below, now that the ban endpoint exists.
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
    public async Task Register_rejects_a_banned_borrower_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        await client.PostAsJsonAsync($"/api/borrowers/{borrowerId}/ban", new { Reason = "Utestengt før lån forsøkt." });

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("BorrowerBanned", problem?.Reason);
    }

    [Fact]
    public async Task MarkLost_sets_the_loan_lost_and_writes_off_the_equipment()
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

        HttpResponseMessage response = await client.PostAsync($"/api/loans/{loan.Id}/mark-lost", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        LoanPayload? updated = await response.Content.ReadFromJsonAsync<LoanPayload>();
        Assert.Equal("Lost", updated?.Status);
        EquipmentStatusPayload? equipment =
            await client.GetFromJsonAsync<EquipmentStatusPayload>($"/api/equipment/{equipmentId}");
        Assert.Equal("WrittenOff", equipment?.Status);
    }

    [Fact]
    public async Task MarkLost_rejects_a_loan_that_is_already_returned_with_409()
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

        HttpResponseMessage response = await client.PostAsync($"/api/loans/{loan.Id}/mark-lost", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("LoanAlreadyClosed", problem?.Reason);
    }

    [Fact]
    public async Task ContactAttempts_can_be_logged_and_read_back()
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

        HttpResponseMessage logResponse = await client.PostAsJsonAsync($"/api/loans/{loan.Id}/contact-attempts", new
        {
            Method = "Phone",
            Outcome = "Ingen svar.",
        });

        Assert.Equal(HttpStatusCode.OK, logResponse.StatusCode);
        List<ContactAttemptPayload>? attempts =
            await client.GetFromJsonAsync<List<ContactAttemptPayload>>($"/api/loans/{loan.Id}/contact-attempts");
        Assert.Contains(attempts!, attempt => attempt.Outcome == "Ingen svar." && attempt.Method == "Phone");
    }

    [Fact]
    public async Task ContactAttempts_return_404_for_a_loan_that_does_not_exist()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync($"/api/loans/{Guid.NewGuid()}/contact-attempts", new
        {
            Method = "Email",
            Outcome = "Lån som ikke finnes.",
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task SendFollowUpEmail_rejects_a_loan_that_is_not_overdue_with_409()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid loanId = await RegisterLoanAsync(client);

        HttpResponseMessage response = await client.PostAsync($"/api/loans/{loanId}/send-followup-email", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        ProblemPayload? problem = await response.Content.ReadFromJsonAsync<ProblemPayload>();
        Assert.Equal("LoanNotOverdue", problem?.Reason);
    }

    [Fact]
    public async Task SendFollowUpEmail_for_an_overdue_loan_logs_a_contact_attempt()
    {
        // Reaching an overdue loan needs a clock past the due date - real
        // time cannot pass it within a test run, see
        // Controllers/OverdueLoanRefreshTests.cs and docs/07-testing.md.
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid loanId = await RegisterLoanAsync(client);

        Guid attemptId;
        using (IServiceScope scope = authenticatedFactory.Services.CreateScope())
        {
            LoanService loanService = await BuildOverdueLoanServiceAsync(scope);
            attemptId = (await loanService.SendFollowUpEmailAsync(loanId, CancellationToken.None)).Id;
        }

        List<ContactAttemptPayload>? attempts =
            await client.GetFromJsonAsync<List<ContactAttemptPayload>>($"/api/loans/{loanId}/contact-attempts");
        Assert.Contains(attempts!, attempt => attempt.Id == attemptId && attempt.Method == "Email");
    }

    [Fact]
    public async Task SendFollowUpEmail_does_not_log_a_contact_attempt_when_the_email_fails_to_send()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new(emailJsStatusCode: HttpStatusCode.BadRequest);
        HttpClient client = authenticatedFactory.CreateClient();
        Guid loanId = await RegisterLoanAsync(client);

        using (IServiceScope scope = authenticatedFactory.Services.CreateScope())
        {
            LoanService loanService = await BuildOverdueLoanServiceAsync(scope);

            DomainConflictException exception = await Assert.ThrowsAsync<DomainConflictException>(
                () => loanService.SendFollowUpEmailAsync(loanId, CancellationToken.None));
            Assert.Equal("EmailSendFailed", exception.Reason);
        }

        List<ContactAttemptPayload>? attempts =
            await client.GetFromJsonAsync<List<ContactAttemptPayload>>($"/api/loans/{loanId}/contact-attempts");
        Assert.Empty(attempts!);
    }

    /// <summary>
    /// A <see cref="LoanService"/> built with a clock 20 days past "now" -
    /// every loan in this file is registered with a 14-day due date, so this
    /// clock reads any of them as overdue without needing real time to pass.
    /// </summary>
    private static async Task<LoanService> BuildOverdueLoanServiceAsync(IServiceScope scope)
    {
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        CurrentUserContext currentUser = new() { StaffId = (await dbContext.Staff.FirstAsync()).Id };
        FollowUpEmailSender emailSender = scope.ServiceProvider.GetRequiredService<FollowUpEmailSender>();
        FakeClock lateClock = new(DateTimeOffset.UtcNow.AddDays(20));

        return new LoanService(dbContext, lateClock, currentUser, emailSender, NullLogger<LoanService>.Instance);
    }

    /// <summary>Registers a fresh borrower, equipment, and a loan due in 14 days.</summary>
    private static async Task<Guid> RegisterLoanAsync(HttpClient client)
    {
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<LoanPayload>())!.Id;
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

    private sealed record ContactAttemptPayload(Guid Id, string Method, string Outcome);
}
