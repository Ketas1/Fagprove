namespace SportForAlle.Api.Configuration;

/// <summary>
/// Configures <see cref="Services.BackgroundJobs.OverdueLoanBackgroundService"/>.
/// Bound from the "OverdueCheck" section - see appsettings.json.
/// </summary>
public class OverdueCheckOptions
{
    public const string SectionName = "OverdueCheck";

    /// <summary>
    /// How often the background job checks for loans past their due date.
    /// Deliberately short (seconds, not minutes) so an overdue transition is
    /// visible almost immediately when demonstrating or testing the system -
    /// not a value tuned for production load, see docs/adr/0011-automatisk-forfall.md.
    /// </summary>
    public int IntervalSeconds { get; set; } = 60;
}
