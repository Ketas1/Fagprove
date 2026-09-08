using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Reports;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;

namespace SportForAlle.Api.Services;

/// <summary>
/// Read-only aggregate queries for the municipality, per "Rapportering" in
/// docs/03-domenemodell.md. Every response is a count or a total, never an
/// individual loan or borrower - see docs/09-lover-og-regler.md.
///
/// There are two reports and they share the same five figures. All five count
/// loans whose <c>StartedAt</c> falls in the period, which is what makes the
/// four sub-counts sum to the total. Counting loans <i>returned</i> late
/// during the period instead would mix two different sets of loans, and the
/// arithmetic would stop working.
/// </summary>
public class ReportService(AppDbContext dbContext)
{
    private static readonly (string Label, int MinAge, int MaxAge)[] _ageGroups =
    [
        ("3-7", 3, 7),
        ("8-12", 8, 12),
        ("13-18", 13, 18),
    ];

    /// <summary>
    /// A day interval over an unbounded period would produce one bucket per
    /// day since the first loan ever registered. Refusing with a 400 that
    /// names the problem beats returning a response nothing can chart.
    /// </summary>
    private const int _maximumBuckets = 400;

    /// <summary>Report 1 of 2. A null <paramref name="from"/> or <paramref name="to"/> means unbounded in that direction.</summary>
    public async Task<LoanSummaryReportResponse> GetLoanSummaryAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        (DateTimeOffset? start, DateTimeOffset? endExclusive) = ToRange(from, to);

        List<(LoanStatus Status, int? DaysLate)> facts = await InPeriod(start, endExclusive)
            .Select(loan => new ValueTuple<LoanStatus, int?>(loan.Status, loan.DaysLate))
            .ToListAsync(cancellationToken);

        return new LoanSummaryReportResponse(from, to, ToFigures(facts));
    }

    /// <summary>
    /// Report 2 of 2. Age is evaluated at <c>Loan.StartedAt</c>, not at report
    /// time, so a borrower who has since had a birthday does not shift which
    /// historical group they are counted in - see docs/03-domenemodell.md.
    /// </summary>
    public async Task<AgeGroupReportResponse> GetAgeGroupsAsync(
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        (DateTimeOffset? start, DateTimeOffset? endExclusive) = ToRange(from, to);

        List<(LoanStatus Status, int? DaysLate, DateTimeOffset StartedAt, DateOnly DateOfBirth)> facts =
            await InPeriod(start, endExclusive)
                .Join(dbContext.Borrowers, loan => loan.BorrowerId, borrower => borrower.Id,
                    (loan, borrower) => new { loan.Status, loan.DaysLate, loan.StartedAt, borrower.DateOfBirth })
                .Select(x => new ValueTuple<LoanStatus, int?, DateTimeOffset, DateOnly>(
                    x.Status, x.DaysLate, x.StartedAt, x.DateOfBirth))
                .ToListAsync(cancellationToken);

        List<AgeGroupFigures> groups = _ageGroups
            .Select(group => new AgeGroupFigures(
                group.Label,
                ToFigures(facts
                    .Where(fact =>
                    {
                        int age = BorrowerRules.CalculateAge(fact.DateOfBirth, fact.StartedAt);
                        return age >= group.MinAge && age <= group.MaxAge;
                    })
                    .Select(fact => (fact.Status, fact.DaysLate)))))
            .ToList();

        return new AgeGroupReportResponse(from, to, groups);
    }

    /// <summary>
    /// Loan counts per day, week or month, for the trend chart. Only
    /// timestamps are read - there is no join to the borrower, so no personal
    /// data is loaded in order to draw a chart.
    /// </summary>
    public async Task<LoanTimelineReportResponse> GetTimelineAsync(
        DateOnly? from, DateOnly? to, TimelineInterval interval, CancellationToken cancellationToken)
    {
        (DateTimeOffset? start, DateTimeOffset? endExclusive) = ToRange(from, to);

        List<DateTimeOffset> startedAt = await InPeriod(start, endExclusive)
            .Select(loan => loan.StartedAt)
            .ToListAsync(cancellationToken);

        List<DateOnly> days = startedAt.Select(value => DateOnly.FromDateTime(value.UtcDateTime)).ToList();

        // Bounded by the requested period when there is one, otherwise by the
        // data itself, so "hele historikken" starts at the first real loan
        // rather than at an invented date. A period the caller asked for
        // explicitly is still filled in with zero buckets when nothing was
        // borrowed in it - "no loans in April" is a real answer, and the chart
        // should show the gap rather than close it.
        DateOnly? first = from ?? (days.Count > 0 ? days.Min() : null);
        DateOnly? last = to ?? (days.Count > 0 ? days.Max() : null);

        if (first is not { } firstDay || last is not { } lastDay)
        {
            return new LoanTimelineReportResponse(from, to, interval, []);
        }

        Dictionary<DateOnly, int> countsByBucket = days
            .GroupBy(day => BucketStart(day, interval))
            .ToDictionary(group => group.Key, group => group.Count());

        List<TimelineBucket> buckets = [];
        for (DateOnly bucket = BucketStart(firstDay, interval); bucket <= lastDay; bucket = Advance(bucket, interval))
        {
            if (buckets.Count == _maximumBuckets)
            {
                throw new ArgumentException(
                    $"The period produces more than {_maximumBuckets} {interval} buckets. Choose a coarser interval.",
                    nameof(interval));
            }

            buckets.Add(new TimelineBucket(bucket, countsByBucket.GetValueOrDefault(bucket)));
        }

        return new LoanTimelineReportResponse(from, to, interval, buckets);
    }

    public async Task<PopularEquipmentReportResponse> GetPopularEquipmentAsync(CancellationToken cancellationToken)
    {
        // Not surfaced by any page - see the "Mest utlånte utstyr" note in
        // docs/03-domenemodell.md. Kept because it is built and tested.
        //
        // Ordering is applied to the grouping, before the final Select into
        // PopularEquipmentItem - not chained after it. EF Core cannot
        // translate a further .OrderBy over members of an already
        // constructor-projected type, the same limitation documented on
        // LoanService's and EquipmentService's own query helpers.
        List<PopularEquipmentItem> items = await dbContext.Loans
            .Join(dbContext.Equipment, loan => loan.EquipmentId, equipment => equipment.Id,
                (loan, equipment) => equipment)
            .Join(dbContext.EquipmentCategories, equipment => equipment.CategoryId, category => category.Id,
                (equipment, category) => new { equipment.Id, equipment.Name, CategoryName = category.Name })
            .GroupBy(x => new { x.Id, x.Name, x.CategoryName })
            .OrderByDescending(group => group.Count())
            .Select(group => new PopularEquipmentItem(group.Key.Id, group.Key.Name, group.Key.CategoryName, group.Count()))
            .ToListAsync(cancellationToken);

        return new PopularEquipmentReportResponse(items);
    }

    private IQueryable<Loan> InPeriod(DateTimeOffset? start, DateTimeOffset? endExclusive)
    {
        IQueryable<Loan> query = dbContext.Loans;

        if (start is { } startValue)
        {
            query = query.Where(loan => loan.StartedAt >= startValue);
        }

        if (endExclusive is { } endValue)
        {
            query = query.Where(loan => loan.StartedAt < endValue);
        }

        return query;
    }

    /// <summary>
    /// <c>DaysLate</c> is null on a loan that was never returned, so the
    /// on-time check has to accept null as well as 0.
    /// </summary>
    private static LoanFigures ToFigures(IEnumerable<(LoanStatus Status, int? DaysLate)> facts)
    {
        List<(LoanStatus Status, int? DaysLate)> materialised = facts.ToList();

        return new LoanFigures(
            materialised.Count,
            materialised.Count(fact => fact.Status == LoanStatus.Returned && fact.DaysLate is null or 0),
            materialised.Count(fact => fact.Status == LoanStatus.Returned && fact.DaysLate > 0),
            materialised.Count(fact => fact.Status is LoanStatus.Overdue or LoanStatus.Lost),
            materialised.Count(fact => fact.Status == LoanStatus.Active));
    }

    private static DateOnly BucketStart(DateOnly date, TimelineInterval interval) => interval switch
    {
        TimelineInterval.Day => date,
        // DayOfWeek puts Sunday at 0; shifting by 6 makes Monday the first day.
        TimelineInterval.Week => date.AddDays(-(((int)date.DayOfWeek + 6) % 7)),
        TimelineInterval.Month => new DateOnly(date.Year, date.Month, 1),
        _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unknown interval."),
    };

    private static DateOnly Advance(DateOnly bucket, TimelineInterval interval) => interval switch
    {
        TimelineInterval.Day => bucket.AddDays(1),
        TimelineInterval.Week => bucket.AddDays(7),
        TimelineInterval.Month => bucket.AddMonths(1),
        _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unknown interval."),
    };

    /// <summary>
    /// Inclusive of both <paramref name="from"/> and <paramref name="to"/> as
    /// whole days. Either may be null, meaning unbounded in that direction.
    /// </summary>
    private static (DateTimeOffset? Start, DateTimeOffset? EndExclusive) ToRange(DateOnly? from, DateOnly? to)
    {
        if (from is { } fromValue && to is { } toValue && toValue < fromValue)
        {
            throw new ArgumentException("'to' cannot be before 'from'.", nameof(to));
        }

        DateTimeOffset? start = from is { } fromDay
            ? new DateTimeOffset(fromDay.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : null;
        DateTimeOffset? endExclusive = to is { } toDay
            ? new DateTimeOffset(toDay.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero)
            : null;

        return (start, endExclusive);
    }
}
