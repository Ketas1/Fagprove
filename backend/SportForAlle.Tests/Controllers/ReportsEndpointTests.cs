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
    private static string Today => DateOnly.FromDateTime(DateTime.UtcNow).ToString("yyyy-MM-dd");

    [Fact]
    public async Task GetLoanSummary_without_a_token_returns_401()
    {
        HttpClient client = factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/reports/loans?from=2026-01-01&to=2026-01-01");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetLoanSummary_increases_by_one_after_registering_a_loan_today()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        LoanSummaryPayload before = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;

        await RegisterLoanAsync(client);

        LoanSummaryPayload after = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Assert.True(after.Figures.TotalLoans >= before.Figures.TotalLoans + 1);
    }

    [Fact]
    public async Task GetLoanSummary_is_zero_for_a_period_with_no_loans()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        await RegisterLoanAsync(client);

        LoanSummaryPayload? report =
            await client.GetFromJsonAsync<LoanSummaryPayload>("/api/reports/loans?from=2000-01-01&to=2000-01-01");

        Assert.Equal(0, report?.Figures.TotalLoans);
        Assert.Equal(0, report?.Figures.StillActive);
    }

    [Fact]
    public async Task GetLoanSummary_rejects_a_to_date_before_from_with_400()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/reports/loans?from=2026-01-10&to=2026-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// The invariant the whole report shape rests on, see
    /// docs/03-domenemodell.md: because every figure counts loans started in
    /// the period, and LoanStatus has exactly four values, the four sub-counts
    /// must account for every loan in the total. One request, so a parallel
    /// test class writing between calls cannot break it.
    /// </summary>
    [Fact]
    public async Task GetLoanSummary_sub_counts_sum_to_the_total()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        await RegisterLoanAsync(client);

        LoanFiguresPayload figures = (await client.GetFromJsonAsync<LoanSummaryPayload>("/api/reports/loans"))!.Figures;

        Assert.Equal(
            figures.TotalLoans,
            figures.ReturnedOnTime + figures.ReturnedLate + figures.NotReturned + figures.StillActive);
        Assert.True(figures.TotalLoans > 0);
    }

    /// <summary>
    /// StillActive and NotReturned are the two figures a parallel test class
    /// can <i>decrement</i> - returning its own loan moves it out of both - so
    /// a before/after delta on them is flaky by construction. The loan this
    /// test registers is its own and nothing else can return it, so asserting
    /// "at least mine" is both robust and still proves the status landed in
    /// the right figure.
    /// </summary>
    [Fact]
    public async Task GetLoanSummary_counts_a_fresh_loan_as_still_active()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        await RegisterLoanAsync(client);

        LoanSummaryPayload after = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Assert.True(after.Figures.StillActive >= 1);
    }

    [Fact]
    public async Task GetLoanSummary_counts_a_late_return_as_returned_late()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        LoanSummaryPayload before = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Guid loanId = await RegisterLoanAsync(client);

        await ReturnLateAsync(authenticatedFactory, loanId);

        LoanSummaryPayload after = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Assert.True(after.Figures.ReturnedLate >= before.Figures.ReturnedLate + 1);
    }

    [Fact]
    public async Task GetLoanSummary_counts_a_return_within_the_due_date_as_on_time()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        LoanSummaryPayload before = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Guid loanId = await RegisterLoanAsync(client);

        HttpResponseMessage returnResponse = await client.PostAsJsonAsync(
            $"/api/loans/{loanId}/return", new { Condition = "Good" });
        returnResponse.EnsureSuccessStatusCode();

        LoanSummaryPayload after = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Assert.True(after.Figures.ReturnedOnTime >= before.Figures.ReturnedOnTime + 1);
    }

    /// <summary>"At least mine" for the same reason as the still-active test above.</summary>
    [Fact]
    public async Task GetLoanSummary_counts_a_lost_loan_as_not_returned()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid loanId = await RegisterLoanAsync(client);

        HttpResponseMessage markLostResponse = await client.PostAsync($"/api/loans/{loanId}/mark-lost", null);
        markLostResponse.EnsureSuccessStatusCode();

        LoanSummaryPayload after = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;
        Assert.True(after.Figures.NotReturned >= 1);
    }

    /// <summary>Omitting both dates means the whole history, not an empty period.</summary>
    [Fact]
    public async Task GetLoanSummary_without_dates_covers_at_least_todays_loans()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        await RegisterLoanAsync(client);
        LoanSummaryPayload today = (await client.GetFromJsonAsync<LoanSummaryPayload>(
            $"/api/reports/loans?from={Today}&to={Today}"))!;

        LoanSummaryPayload allTime = (await client.GetFromJsonAsync<LoanSummaryPayload>("/api/reports/loans"))!;

        Assert.Null(allTime.From);
        Assert.Null(allTime.To);
        Assert.True(allTime.Figures.TotalLoans >= today.Figures.TotalLoans);
    }

    [Fact]
    public async Task GetAgeGroups_returns_the_three_fixed_groups()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        AgeGroupReportPayload report =
            (await client.GetFromJsonAsync<AgeGroupReportPayload>("/api/reports/age-groups"))!;

        Assert.Equal(["3-7", "8-12", "13-18"], report.Groups.Select(group => group.AgeGroup));
    }

    [Fact]
    public async Task GetAgeGroups_counts_a_ten_year_old_in_the_middle_group()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        int countBefore = await AgeGroupTotalAsync(client, "8-12");

        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(
            client, dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-10)));
        await RegisterLoanAsync(client, borrowerId);

        Assert.True(await AgeGroupTotalAsync(client, "8-12") >= countBefore + 1);
    }

    /// <summary>
    /// The boundary the 2026-09-07 correction moved: seven now belongs to the
    /// youngest group, not the middle one. See docs/03-domenemodell.md.
    /// </summary>
    [Fact]
    public async Task GetAgeGroups_counts_a_seven_year_old_in_the_youngest_group()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        int countBefore = await AgeGroupTotalAsync(client, "3-7");

        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(
            client, dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-7).AddDays(-1)));
        await RegisterLoanAsync(client, borrowerId);

        Assert.True(await AgeGroupTotalAsync(client, "3-7") >= countBefore + 1);
    }

    [Fact]
    public async Task GetAgeGroups_counts_an_eight_year_old_in_the_middle_group()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        int countBefore = await AgeGroupTotalAsync(client, "8-12");

        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(
            client, dateOfBirth: DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-8).AddDays(-1)));
        await RegisterLoanAsync(client, borrowerId);

        Assert.True(await AgeGroupTotalAsync(client, "8-12") >= countBefore + 1);
    }

    /// <summary>Summing one figure across the groups must match the summary report's own number.</summary>
    [Fact]
    public async Task GetAgeGroups_totals_match_the_summary_report()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        await RegisterLoanAsync(client);
        // A closed historical window nothing else can write into, so the two
        // requests below cannot disagree because of a parallel test class.
        const string Range = "from=2020-01-01&to=2020-12-31";

        LoanSummaryPayload summary = (await client.GetFromJsonAsync<LoanSummaryPayload>($"/api/reports/loans?{Range}"))!;
        AgeGroupReportPayload byAge =
            (await client.GetFromJsonAsync<AgeGroupReportPayload>($"/api/reports/age-groups?{Range}"))!;

        Assert.Equal(summary.Figures.TotalLoans, byAge.Groups.Sum(group => group.Figures.TotalLoans));
    }

    [Fact]
    public async Task GetTimeline_defaults_to_month_buckets()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        TimelinePayload report = (await client.GetFromJsonAsync<TimelinePayload>(
            $"/api/reports/timeline?from={Today}&to={Today}"))!;

        Assert.Equal("Month", report.Interval);
    }

    /// <summary>A quiet period is a real answer - the buckets exist with a count of 0.</summary>
    [Fact]
    public async Task GetTimeline_fills_an_empty_period_with_zero_buckets()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        TimelinePayload report = (await client.GetFromJsonAsync<TimelinePayload>(
            "/api/reports/timeline?from=2000-01-01&to=2000-03-31&interval=Month"))!;

        Assert.Equal(3, report.Buckets.Count);
        Assert.All(report.Buckets, bucket => Assert.Equal(0, bucket.Count));
        Assert.Equal([new DateOnly(2000, 1, 1), new DateOnly(2000, 2, 1), new DateOnly(2000, 3, 1)],
            report.Buckets.Select(bucket => bucket.BucketStart));
    }

    [Fact]
    public async Task GetTimeline_counts_a_loan_registered_today_in_a_single_day_bucket()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        TimelinePayload before = (await client.GetFromJsonAsync<TimelinePayload>(
            $"/api/reports/timeline?from={Today}&to={Today}&interval=Day"))!;

        await RegisterLoanAsync(client);

        TimelinePayload after = (await client.GetFromJsonAsync<TimelinePayload>(
            $"/api/reports/timeline?from={Today}&to={Today}&interval=Day"))!;
        Assert.Single(after.Buckets);
        Assert.True(after.Buckets[0].Count >= before.Buckets[0].Count + 1);
    }

    [Fact]
    public async Task GetTimeline_week_buckets_start_on_a_monday()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        TimelinePayload report = (await client.GetFromJsonAsync<TimelinePayload>(
            "/api/reports/timeline?from=2000-01-05&to=2000-02-05&interval=Week"))!;

        Assert.NotEmpty(report.Buckets);
        Assert.All(report.Buckets, bucket => Assert.Equal(DayOfWeek.Monday, bucket.BucketStart.DayOfWeek));
    }

    /// <summary>
    /// Day buckets over 25 years would be a response nothing can chart, so the
    /// service refuses instead of building it - see _maximumBuckets.
    /// </summary>
    [Fact]
    public async Task GetTimeline_rejects_a_period_with_too_many_buckets_with_400()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response =
            await client.GetAsync($"/api/reports/timeline?from=2000-01-01&to={Today}&interval=Day");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// Replaced by the NotReturned and ReturnedLate figures on the summary
    /// report, which are anchored to the period instead of counting across all
    /// history. Asserting the removal keeps the two from quietly coming back
    /// and disagreeing with each other.
    /// </summary>
    [Fact]
    public async Task GetOverdueSummary_no_longer_exists()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/reports/overdue-summary");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

    private static async Task<int> AgeGroupTotalAsync(HttpClient client, string ageGroup)
    {
        AgeGroupReportPayload report = (await client.GetFromJsonAsync<AgeGroupReportPayload>(
            $"/api/reports/age-groups?from={Today}&to={Today}"))!;

        return report.Groups.Single(group => group.AgeGroup == ageGroup).Figures.TotalLoans;
    }

    /// <summary>
    /// Real time cannot pass a due date within a test run, so a clock moved
    /// past it stands in - see docs/07-testing.md.
    /// </summary>
    private static async Task ReturnLateAsync(
        AuthenticatedWebApplicationFactory<Program> authenticatedFactory, Guid loanId)
    {
        using IServiceScope scope = authenticatedFactory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // A manually created scope never runs RequireLinkedStaffMiddleware, so
        // CurrentUserContext.StaffId must be populated by hand - any real
        // Staff row satisfies the foreign key.
        CurrentUserContext currentUser = new() { StaffId = (await dbContext.Staff.FirstAsync()).Id };
        FollowUpEmailSender emailSender = scope.ServiceProvider.GetRequiredService<FollowUpEmailSender>();
        FakeClock lateClock = new(DateTimeOffset.UtcNow.AddDays(20));
        LoanService loanService = new(
            dbContext, lateClock, currentUser, emailSender, NullLogger<LoanService>.Instance);

        await loanService.ReturnAsync(
            loanId, new ReturnLoanRequest { Condition = EquipmentCondition.Good }, CancellationToken.None);
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

    private sealed record LoanFiguresPayload(
        int TotalLoans, int ReturnedOnTime, int ReturnedLate, int NotReturned, int StillActive);

    private sealed record LoanSummaryPayload(DateOnly? From, DateOnly? To, LoanFiguresPayload Figures);

    private sealed record AgeGroupFiguresPayload(string AgeGroup, LoanFiguresPayload Figures);

    private sealed record AgeGroupReportPayload(DateOnly? From, DateOnly? To, List<AgeGroupFiguresPayload> Groups);

    private sealed record TimelineBucketPayload(DateOnly BucketStart, int Count);

    private sealed record TimelinePayload(
        DateOnly? From, DateOnly? To, string Interval, List<TimelineBucketPayload> Buckets);

    private sealed record PopularEquipmentItemPayload(Guid EquipmentId, string EquipmentName, string CategoryName, int LoanCount);

    private sealed record PopularEquipmentReportPayload(List<PopularEquipmentItemPayload> Items);
}
