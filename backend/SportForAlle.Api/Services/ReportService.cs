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
/// </summary>
public class ReportService(AppDbContext dbContext)
{
    private static readonly (string Label, int MinAge, int MaxAge)[] _ageGroups =
    [
        ("3-6", 3, 6),
        ("7-12", 7, 12),
        ("13-18", 13, 18),
    ];

    public async Task<LoanCountReportResponse> GetLoanCountAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        (DateTimeOffset start, DateTimeOffset endExclusive) = ToRange(from, to);

        int totalLoans = await dbContext.Loans
            .CountAsync(loan => loan.StartedAt >= start && loan.StartedAt < endExclusive, cancellationToken);

        return new LoanCountReportResponse(from, to, totalLoans);
    }

    /// <summary>
    /// Age is evaluated at <c>Loan.StartedAt</c>, not at report time, so a
    /// borrower who has since had a birthday does not shift which historical
    /// period they are counted in - see docs/03-domenemodell.md.
    /// </summary>
    public async Task<AgeGroupReportResponse> GetAgeGroupsAsync(DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        (DateTimeOffset start, DateTimeOffset endExclusive) = ToRange(from, to);

        List<(DateTimeOffset StartedAt, DateOnly DateOfBirth)> loans = await dbContext.Loans
            .Where(loan => loan.StartedAt >= start && loan.StartedAt < endExclusive)
            .Join(dbContext.Borrowers, loan => loan.BorrowerId, borrower => borrower.Id,
                (loan, borrower) => new { loan.StartedAt, borrower.DateOfBirth })
            .Select(x => new ValueTuple<DateTimeOffset, DateOnly>(x.StartedAt, x.DateOfBirth))
            .ToListAsync(cancellationToken);

        List<AgeGroupCount> groups = _ageGroups
            .Select(group =>
            {
                int count = loans.Count(loan =>
                {
                    int age = BorrowerRules.CalculateAge(loan.DateOfBirth, loan.StartedAt);
                    return age >= group.MinAge && age <= group.MaxAge;
                });

                return new AgeGroupCount(group.Label, count);
            })
            .ToList();

        return new AgeGroupReportResponse(from, to, groups);
    }

    public async Task<PopularEquipmentReportResponse> GetPopularEquipmentAsync(CancellationToken cancellationToken)
    {
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

    public async Task<OverdueSummaryReportResponse> GetOverdueSummaryAsync(CancellationToken cancellationToken)
    {
        int lateReturnedCount = await dbContext.Loans
            .CountAsync(loan => loan.DaysLate > 0, cancellationToken);

        int undeliveredCount = await dbContext.Loans
            .CountAsync(loan => loan.Status == LoanStatus.Overdue || loan.Status == LoanStatus.Lost, cancellationToken);

        return new OverdueSummaryReportResponse(lateReturnedCount, undeliveredCount);
    }

    /// <summary>Inclusive of both <paramref name="from"/> and <paramref name="to"/> as whole days.</summary>
    private static (DateTimeOffset Start, DateTimeOffset EndExclusive) ToRange(DateOnly from, DateOnly to)
    {
        if (to < from)
        {
            throw new ArgumentException("'to' cannot be before 'from'.", nameof(to));
        }

        DateTimeOffset start = new(from.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        DateTimeOffset endExclusive = new(to.AddDays(1).ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        return (start, endExclusive);
    }
}
