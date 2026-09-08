using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Services.Rules;

public class LoanRulesTests
{
    private static Borrower CreateActiveBorrower(FakeClock clock) =>
        new("Test Testesen", DateOnly.FromDateTime(clock.UtcNow.UtcDateTime).AddYears(-10), Guid.NewGuid(), clock, null);

    private static Equipment CreateAvailableEquipment(FakeClock clock) =>
        new("Ski", Guid.NewGuid().ToString(), Guid.NewGuid(), EquipmentCondition.Good, clock, null);

    [Fact]
    public void EnsureBorrowerCanBorrow_allows_a_borrower_with_no_open_loans()
    {
        FakeClock clock = new();
        Borrower borrower = CreateActiveBorrower(clock);

        LoanRules.EnsureBorrowerCanBorrow(borrower, [], clock);
    }

    [Fact]
    public void EnsureBorrowerCanBorrow_rejects_a_banned_borrower()
    {
        FakeClock clock = new();
        Borrower borrower = CreateActiveBorrower(clock);
        borrower.Ban("Ikke levert utstyr", clock, null);

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => LoanRules.EnsureBorrowerCanBorrow(borrower, [], clock));

        Assert.Equal("BorrowerBanned", exception.Reason);
    }

    [Fact]
    public void EnsureBorrowerCanBorrow_rejects_a_borrower_with_an_open_overdue_loan()
    {
        FakeClock clock = new();
        Borrower borrower = CreateActiveBorrower(clock);
        Loan overdueLoan = new(borrower.Id, Guid.NewGuid(), clock.UtcNow + TimeSpan.FromDays(1), clock, null);
        clock.Advance(TimeSpan.FromDays(2));

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => LoanRules.EnsureBorrowerCanBorrow(borrower, [overdueLoan], clock));

        Assert.Equal("BorrowerHasOverdueLoan", exception.Reason);
    }

    [Fact]
    public void EnsureBorrowerCanBorrow_allows_a_borrower_whose_open_loan_is_not_yet_due()
    {
        FakeClock clock = new();
        Borrower borrower = CreateActiveBorrower(clock);
        Loan activeLoan = new(borrower.Id, Guid.NewGuid(), clock.UtcNow + TimeSpan.FromDays(14), clock, null);

        LoanRules.EnsureBorrowerCanBorrow(borrower, [activeLoan], clock);
    }

    [Fact]
    public void EnsureLoanIsOverdue_allows_an_overdue_loan()
    {
        FakeClock clock = new();
        Loan loan = new(Guid.NewGuid(), Guid.NewGuid(), clock.UtcNow + TimeSpan.FromDays(1), clock, null);
        clock.Advance(TimeSpan.FromDays(2));

        LoanRules.EnsureLoanIsOverdue(loan, clock);
    }

    [Fact]
    public void EnsureLoanIsOverdue_rejects_a_loan_that_is_not_yet_due()
    {
        FakeClock clock = new();
        Loan loan = new(Guid.NewGuid(), Guid.NewGuid(), clock.UtcNow + TimeSpan.FromDays(14), clock, null);

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => LoanRules.EnsureLoanIsOverdue(loan, clock));

        Assert.Equal("LoanNotOverdue", exception.Reason);
    }

    [Fact]
    public void EnsureEquipmentAvailable_allows_available_equipment()
    {
        FakeClock clock = new();
        Equipment equipment = CreateAvailableEquipment(clock);

        LoanRules.EnsureEquipmentAvailable(equipment);
    }

    [Fact]
    public void EnsureEquipmentAvailable_rejects_equipment_already_on_loan()
    {
        FakeClock clock = new();
        Equipment equipment = CreateAvailableEquipment(clock);
        equipment.MarkOnLoan(clock, null);

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => LoanRules.EnsureEquipmentAvailable(equipment));

        Assert.Equal("EquipmentNotAvailable", exception.Reason);
    }
}
