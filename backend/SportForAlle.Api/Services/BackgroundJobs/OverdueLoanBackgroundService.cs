using Microsoft.Extensions.Options;
using SportForAlle.Api.Configuration;

namespace SportForAlle.Api.Services.BackgroundJobs;

/// <summary>
/// The write-time half of ADR-0011: on a timer, materialises <see cref="Models.LoanStatus.Overdue"/>
/// for every loan past its due date via <see cref="LoanService.RefreshOverdueLoansAsync"/>, so
/// business rule 7 in docs/03-domenemodell.md ("forfall oppdages automatisk") holds without staff
/// having to check manually.
/// </summary>
/// <remarks>
/// Registered as a singleton hosted service, but <see cref="LoanService"/> and its
/// <c>AppDbContext</c> are scoped - a new <see cref="IServiceScope"/> is created for every tick
/// so each run gets its own DbContext, the same way a web request would.
/// </remarks>
public class OverdueLoanBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<OverdueCheckOptions> options,
    ILogger<OverdueLoanBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(options.Value.IntervalSeconds));

        do
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                LoanService loanService = scope.ServiceProvider.GetRequiredService<LoanService>();

                int updated = await loanService.RefreshOverdueLoansAsync(stoppingToken);

                // One line only when something actually changed - see CLAUDE.md,
                // "Logging: one deliberate line beats a firehose."
                if (updated > 0)
                {
                    logger.LogInformation("Overdue check materialised {Count} loan(s) as Overdue.", updated);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // A single failed tick (a transient database blip, for example) must not
                // take the whole background loop down - the next tick tries again.
                logger.LogError(exception, "Overdue check failed; will retry on the next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
