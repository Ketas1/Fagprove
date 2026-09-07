using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Services;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Controllers;

/// <summary>
/// Exercises the write-time half of ADR-0011 - <see cref="LoanService.RefreshOverdueLoansAsync"/>,
/// the method <see cref="SportForAlle.Api.Services.BackgroundJobs.OverdueLoanBackgroundService"/>
/// calls on a timer - against the real database. The timer itself is not exercised here: waiting on
/// real wall-clock time is exactly what docs/07-testing.md's injected-clock rule exists to avoid, so
/// a <see cref="FakeClock"/> is used to jump past the due date instead.
/// </summary>
public class OverdueLoanRefreshTests
{
    [Fact]
    public async Task RefreshOverdueLoansAsync_materialises_overdue_for_a_loan_past_its_due_date()
    {
        await using AuthenticatedWebApplicationFactory<Program> authenticatedFactory = new();
        HttpClient client = authenticatedFactory.CreateClient();
        Guid borrowerId = await ApiTestDataBuilder.CreateBorrowerAsync(client);
        Guid equipmentId = await ApiTestDataBuilder.CreateEquipmentAsync(client);
        HttpResponseMessage registerResponse = await client.PostAsJsonAsync("/api/loans", new
        {
            BorrowerId = borrowerId,
            EquipmentId = equipmentId,
            DueDate = DateTimeOffset.UtcNow.AddSeconds(5),
        });
        LoanPayload loan = (await registerResponse.Content.ReadFromJsonAsync<LoanPayload>())!;

        int updated = await RunRefreshAsync(authenticatedFactory, new FakeClock(DateTimeOffset.UtcNow.AddDays(1)));

        Assert.True(updated >= 1);
        LoanPayload? refreshed = await client.GetFromJsonAsync<LoanPayload>($"/api/loans/{loan.Id}");
        Assert.Equal("Overdue", refreshed?.Status);
    }

    [Fact]
    public async Task RefreshOverdueLoansAsync_does_not_touch_a_loan_that_is_not_due_yet()
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

        await RunRefreshAsync(authenticatedFactory, new FakeClock());

        LoanPayload? refreshed = await client.GetFromJsonAsync<LoanPayload>($"/api/loans/{loan.Id}");
        Assert.Equal("Active", refreshed?.Status);
    }

    private static async Task<int> RunRefreshAsync(WebApplicationFactory<Program> factory, FakeClock clock)
    {
        using IServiceScope scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        CurrentUserContext currentUser = scope.ServiceProvider.GetRequiredService<CurrentUserContext>();
        FollowUpEmailSender emailSender = scope.ServiceProvider.GetRequiredService<FollowUpEmailSender>();
        LoanService loanService = new(dbContext, clock, currentUser, emailSender, NullLogger<LoanService>.Instance);

        return await loanService.RefreshOverdueLoansAsync(CancellationToken.None);
    }

    private sealed record LoanPayload(Guid Id, string Status);
}
