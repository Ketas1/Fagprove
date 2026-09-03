using SportForAlle.Api.Helpers;

namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// A clock tests can move forward, so overdue and late-return logic can be
/// verified without waiting on real time. See docs/07-testing.md.
/// </summary>
public class FakeClock(DateTimeOffset? now = null) : IClock
{
    public DateTimeOffset UtcNow { get; private set; } = now ?? new DateTimeOffset(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    public void Advance(TimeSpan duration) => UtcNow += duration;

    public void Set(DateTimeOffset value) => UtcNow = value;
}
