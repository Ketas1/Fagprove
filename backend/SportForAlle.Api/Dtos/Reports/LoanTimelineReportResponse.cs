namespace SportForAlle.Api.Dtos.Reports;

/// <summary>Bucket size for the trend chart on the reports page.</summary>
public enum TimelineInterval
{
    Day,
    Week,
    Month,
}

/// <param name="BucketStart">First day of the bucket - the Monday for a week, the 1st for a month.</param>
public record TimelineBucket(DateOnly BucketStart, int Count);

/// <summary>
/// A breakdown of <see cref="LoanFigures.TotalLoans"/> over time, not a third
/// report - it exists so the reports page can draw a bar chart. Buckets with
/// no loans are included with a count of 0, so a quiet month is visible as a
/// gap in the chart rather than silently disappearing.
/// </summary>
public record LoanTimelineReportResponse(
    DateOnly? From,
    DateOnly? To,
    TimelineInterval Interval,
    IReadOnlyList<TimelineBucket> Buckets);
