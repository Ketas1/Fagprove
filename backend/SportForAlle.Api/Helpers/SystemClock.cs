namespace SportForAlle.Api.Helpers;

/// <summary>The real clock, used everywhere outside tests.</summary>
public class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
