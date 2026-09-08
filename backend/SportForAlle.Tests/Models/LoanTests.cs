using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class LoanTests
{
    private static Loan CreateLoan(FakeClock clock, TimeSpan? period = null) =>
        new(Guid.NewGuid(), Guid.NewGuid(), clock.UtcNow + (period ?? TimeSpan.FromDays(14)), clock, Guid.NewGuid());

    [Fact]
    public void Constructor_starts_active()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock);

        Assert.Equal(LoanStatus.Active, loan.Status);
        Assert.Null(loan.ReturnedAt);
        Assert.Null(loan.DaysLate);
    }

    [Fact]
    public void Constructor_rejects_a_due_date_that_is_not_after_the_start_date()
    {
        FakeClock clock = new();

        Assert.Throws<ArgumentException>(() => new Loan(Guid.NewGuid(), Guid.NewGuid(), clock.UtcNow, clock, null));
    }

    [Fact]
    public void IsOverdueNow_is_false_before_the_due_date()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock);

        Assert.False(loan.IsOverdueNow(clock));
    }

    [Fact]
    public void IsOverdueNow_is_false_when_due_earlier_the_same_calendar_day()
    {
        // The reported bug: a loan due "today" at any earlier time-of-day
        // must not read as overdue while it is still "today" - only the
        // calendar date matters, not the stored time. See the 2026-09-07
        // addendum to ADR-0011.
        FakeClock clock = new(new DateTimeOffset(2026, 1, 2, 8, 0, 0, TimeSpan.Zero));
        Loan loan = new(Guid.NewGuid(), Guid.NewGuid(), new DateTimeOffset(2026, 1, 2, 9, 0, 0, TimeSpan.Zero), clock, null);

        clock.Set(new DateTimeOffset(2026, 1, 2, 23, 0, 0, TimeSpan.Zero));

        Assert.False(loan.IsOverdueNow(clock));
    }

    [Fact]
    public void IsOverdueNow_is_true_once_the_due_date_has_passed_even_without_a_background_job()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(1));

        clock.Advance(TimeSpan.FromDays(2));

        Assert.True(loan.IsOverdueNow(clock));
        Assert.Equal(LoanStatus.Active, loan.Status);
    }

    [Fact]
    public void IsOverdueNow_stays_true_once_status_has_already_been_materialised_to_overdue()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(1));
        clock.Advance(TimeSpan.FromDays(2));
        loan.RefreshOverdueStatus(clock);

        // Once Status is materialised to Overdue, IsOverdueNow must keep
        // reporting true - the blocked-loan check (business rule 2) relies
        // on it regardless of whether a background job has run yet.
        Assert.Equal(LoanStatus.Overdue, loan.Status);
        Assert.True(loan.IsOverdueNow(clock));
    }

    [Fact]
    public void RefreshOverdueStatus_materialises_overdue_once_the_due_date_has_passed()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(1));
        clock.Advance(TimeSpan.FromDays(2));

        loan.RefreshOverdueStatus(clock);

        Assert.Equal(LoanStatus.Overdue, loan.Status);
    }

    [Fact]
    public void RefreshOverdueStatus_does_nothing_before_the_due_date()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock);

        loan.RefreshOverdueStatus(clock);

        Assert.Equal(LoanStatus.Active, loan.Status);
    }

    [Fact]
    public void Return_on_time_sets_zero_days_late()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(7));

        loan.Return(clock, Guid.NewGuid());

        Assert.Equal(LoanStatus.Returned, loan.Status);
        Assert.Equal(0, loan.DaysLate);
        Assert.NotNull(loan.ReturnedAt);
    }

    [Fact]
    public void Return_after_the_due_date_computes_days_late()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(1));
        clock.Advance(TimeSpan.FromDays(4));

        loan.Return(clock, Guid.NewGuid());

        Assert.Equal(3, loan.DaysLate);
    }

    [Fact]
    public void Return_works_from_overdue_as_well_as_active()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock, TimeSpan.FromDays(1));
        clock.Advance(TimeSpan.FromDays(2));
        loan.RefreshOverdueStatus(clock);

        loan.Return(clock, Guid.NewGuid());

        Assert.Equal(LoanStatus.Returned, loan.Status);
    }

    [Fact]
    public void Return_rejects_a_loan_that_is_already_returned()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock);
        loan.Return(clock, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => loan.Return(clock, Guid.NewGuid()));
    }

    [Fact]
    public void MarkLost_is_terminal()
    {
        FakeClock clock = new();
        Loan loan = CreateLoan(clock);

        loan.MarkLost(clock, Guid.NewGuid());

        Assert.Equal(LoanStatus.Lost, loan.Status);
        Assert.Throws<InvalidOperationException>(() => loan.Return(clock, Guid.NewGuid()));
    }
}
