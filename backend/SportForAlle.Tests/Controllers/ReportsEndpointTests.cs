using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// The database is shared across the whole test run, and xUnit runs different
/// test classes in parallel by default (docs/07-testing.md does not change
/// either), so most assertions here compare a report's result before and
/// after creating one known piece of data using "at least" rather than an
/// exact delta - another test class registering a loan (or marking one lost)
/// at the same moment is expected, not a bug. Where a result cannot be
/// polluted by other tests at all (a specific, freshly created equipment id,
/// or a date range far outside "today"), the exact value is asserted instead.
/// </summary>
public class ReportsEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    [Fact]
    public async Task GetLoanCount_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/reports/loans?from=2026-01-01&to=2026-01-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLoanCount_increases_by_one_after_registering_a_loan_today()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        string today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        LoanCountPayload before = (await client.GetFromJsonAsync<LoanCountPayload>(
            $"/api/reports/loans?from={today}&to={today}"))!;

        await RegisterLoanAsync(client);

        LoanCountPayload after = (await client.GetFromJsonAsync<LoanCountPayload>(
            $"/api/reports/loans?from={today}&to={today}"))!;
        Assert.True(after.TotalLoans >= before.TotalLoans + 1);
    }

    [Fact]
    public async Task GetLoanCount_is_zero_for_a_period_with_no_loans()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        await RegisterLoanAsync(client);

        LoanCountPayload? report =
            await client.GetFromJsonAsync<LoanCountPayload>("/api/reports/loans?from=2000-01-01&to=2000-01-01");

        Assert.Equal(0, report?.TotalLoans);
    }

    [Fact]
    public async Task GetLoanCount_rejects_a_to_date_before_from_with_400()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/reports/loans?from=2026-01-10&to=2026-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAgeGroups_counts_a_new_loan_in_the_borrowers_age_group()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        string today = DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");
        AgeGroupReportPayload before = (await client.GetFromJsonAsync<AgeGroupReportPayload>(
            $"/api/reports/age-groups?from={today}&to={today}"))!;
        int countBefore = before.Groups.Single(g => g.AgeGroup == "7-12").Count;

        // Ten years old at registration - squarely inside the 7-12 bucket.
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(
            client, dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)));
        await RegisterLoanAsync(client, borrowerId);

        AgeGroupReportPayload after = (await client.GetFromJsonAsync<AgeGroupReportPayload>(
            $"/api/reports/age-groups?from={today}&to={today}"))!;
        int countAfter = after.Groups.Single(g => g.AgeGroup == "7-12").Count;
        Assert.True(countAfter >= countBefore + 1);
    }

    [Fact]
    public async Task GetPopularEquipment_counts_loans_for_a_specific_item()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        registerResponse.EnsureSuccessStatusCode();

        PopularEquipmentReportPayload report =
            (await client.GetFromJsonAsync<PopularEquipmentReportPayload>("/api/reports/popular-equipment"))!;

        PopularEquipmentItemPayload item = report.Items.Single(i => i.EquipmentId == equipmentId);
        Assert.Equal(1, item.LoanCount);
    }

    [Fact]
    public async Task GetOverdueSummary_counts_a_loan_returned_after_its_due_date()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        OverdueSummaryPayload before =
            (await client.GetFromJsonAsync<OverdueSummaryPayload>("/api/reports/overdue-summary"))!;
        Guid loanId = await RegisterLoanAsync(client);

        using (IServiceScope scope = authenticatedFactory.Services.CreateScope())
        {
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            // A manually created scope never runs RequireLinkedStaffMiddleware,
            // so CurrentUserContext.StaffId must be populated by hand - any
            // real Staff row satisfies the foreign key.
            CurrentUserContext currentUser = new() { StaffId = (await dbContext.Staff.FirstAsync()).Id };
            FollowUpEmailSender emailSender = scope.ServiceProvider.GetRequiredService<FollowUpEmailSender>();
            // Real time cannot pass the due date within a test run, so a clock
            // moved past it stands in - see docs/07-testing.md.
            FakeClock lateClock = new(DateTimeOffset.UtcNow.AddDays(20));
            LoanService loanService = new(dbContext, lateClock, currentUser, emailSender, NullLogger<LoanService>.Instance);

            await loanService.ReturnAsync(loanId, new ReturnLoanRequest { Condition = EquipmentCondition.Good }, CancellationToken.None);
        }

        OverdueSummaryPayload after =
            (await client.GetFromJsonAsync<OverdueSummaryPayload>("/api/reports/overdue-summary"))!;
        Assert.True(after.LateReturnedCount >= before.LateReturnedCount + 1);
    }

    [Fact]
    public async Task GetOverdueSummary_counts_a_lost_loan_as_undelivered()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        OverdueSummaryPayload before =
            (await client.GetFromJsonAsync<OverdueSummaryPayload>("/api/reports/overdue-summary"))!;
        Guid loanId = await RegisterLoanAsync(client);

        HttpResponseMessage markLostResponse = await client.PostAsync($"/api/loans/{loanId}/mark-lost", null);
        markLostResponse.EnsureSuccessStatusCode();

        OverdueSummaryPayload after =
            (await client.GetFromJsonAsync<OverdueSummaryPayload>("/api/reports/overdue-summary"))!;
        Assert.True(after.UndeliveredCount >= before.UndeliveredCount + 1);
    }

    private static async Task<Guid> RegisterLoanAsync(HttpClient client, Guid? borrowerId = null)
    {
        Guid resolvedBorrowerId = borrowerId ?? await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = resolvedBorrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddDays(14),
        });
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<LoanIdPayload>())!.Id;
    }

    private sealed record LoanIdPayload(Guid Id);

    private sealed record LoanCountPayload(DateOnly From, DateOnly To, int TotalLoans);

    private sealed record AgeGroupCountPayload(string AgeGroup, int Count);

    private sealed record AgeGroupReportPayload(DateOnly From, DateOnly To, List<AgeGroupCountPayload> Groups);

    private sealed record PopularEquipmentItemPayload(Guid EquipmentId, string EquipmentName, string CategoryName, int LoanCount);

    private sealed record PopularEquipmentReportPayload(List<PopularEquipmentItemPayload> Items);

    private sealed record OverdueSummaryPayload(int LateReturnedCount, int UndeliveredCount);
}
