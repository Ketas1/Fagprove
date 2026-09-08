using SportForAlle.Api.Helpers;
using SportForAlle.Api.Models;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services.Rules;

/// <summary>
/// Business rules 2 and 3 in docs/03-domenemodell.md - the core incentive
/// mechanism this system exists for. Operates on entities already loaded by
/// <c>LoanService</c>, so it can be unit tested without a database.
/// </summary>
public static class LoanRules
{
    /// <param name="borrower">The borrower requesting a new loan.</param>
    /// <param name="openLoans">
    /// The borrower's loans with <see cref="LoanStatus.Active"/> or
    /// <see cref="LoanStatus.Overdue"/> status - closed loans are irrelevant
    /// to this check.
    /// </param>
    /// <param name="clock">
    /// Used to evaluate <see cref="Loan.IsOverdueNow"/>, so a loan that has
    /// passed its due date blocks a new one even if a background job has
    /// not yet materialised its status - see ADR-0011.
    /// </param>
    public static void EnsureBorrowerCanBorrow(Borrower borrower, IReadOnlyCollection<Loan> openLoans, IClock clock)
    {
        if (borrower.Status == BorrowerStatus.Banned)
        {
            throw new DomainConflictException("BorrowerBanned", "Låntakeren er utestengt.");
        }

        if (openLoans.Any(loan => loan.IsOverdueNow(clock)))
        {
            throw new DomainConflictException("BorrowerHasOverdueLoan", "Låntakeren har et åpent forfalt lån.");
        }
    }

    /// <summary>
    /// A Returned or Lost loan is history the reports are built on. Its
    /// DaysLate and the borrower's late-return counter were computed at the
    /// time it closed, so re-opening it for edits would silently invalidate
    /// both. Notes can still be added to a closed loan - see ADR-0025.
    /// </summary>
    public static void EnsureLoanCanBeCorrected(Loan loan)
    {
        if (loan.Status is LoanStatus.Returned or LoanStatus.Lost)
        {
            throw new DomainConflictException(
                "LoanAlreadyClosed", "Utlånet er avsluttet og kan ikke endres.");
        }
    }

    public static void EnsureEquipmentAvailable(Equipment equipment)
    {
        if (equipment.Status != EquipmentStatus.Available)
        {
            throw new DomainConflictException("EquipmentNotAvailable", "Utstyret er ikke ledig.");
        }
    }

    /// <summary>
    /// The follow-up email exists to chase an overdue loan - sending it for
    /// a loan that isn't overdue would be a false accusation to the
    /// guardian, not just a wasted email.
    /// </summary>
    public static void EnsureLoanIsOverdue(Loan loan, IClock clock)
    {
        if (!loan.IsOverdueNow(clock))
        {
            throw new DomainConflictException("LoanNotOverdue", "Lånet er ikke forfalt.");
        }
    }
}
