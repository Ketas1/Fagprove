namespace SportForAlle.Api.Helpers;

/// <summary>
/// Abstraction over the current time. Entities and services depend on this
/// instead of reading <see cref="DateTimeOffset.UtcNow"/> directly, so tests
/// can move time forward and verify overdue and late-return logic without
/// waiting on a clock.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
