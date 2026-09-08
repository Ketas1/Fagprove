using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// Links one borrower to one piece of equipment for a period. See the
/// "Lånestatus" state machine in docs/03-domenemodell.md, and ADR-0011 for
/// why overdue is both computed here and materialised separately by a
/// background job (not built in this change).
/// </summary>
public class Loan : AuditableEntity
{
    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Loan()
    {
    }

    public Loan(Guid borrowerId, Guid equipmentId, DateTimeOffset dueDate, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        BorrowerId = borrowerId;
        EquipmentId = equipmentId;
        StartedAt = clock.UtcNow;

        if (dueDate <= StartedAt)
        {
            throw new ArgumentException("Loan due date must be after the start date.", nameof(dueDate));
        }

        // Normalised to UTC: the instant is unchanged, but Npgsql refuses to
        // write any offset other than 0 to a 'timestamp with time zone'.
        DueDate = dueDate.ToUniversalTime();
        Status = LoanStatus.Active;
    }

    public Guid BorrowerId { get; private set; }

    public Guid EquipmentId { get; private set; }

    public DateTimeOffset StartedAt { get; private set; }

    public DateTimeOffset DueDate { get; private set; }

    /// <summary>Null while the loan is still open.</summary>
    public DateTimeOffset? ReturnedAt { get; private set; }

    /// <summary>Set when the loan is returned: 0 if on time, otherwise the number of days late.</summary>
    public int? DaysLate { get; private set; }

    public LoanStatus Status { get; private set; }

    /// <summary>
    /// True if the due date has passed and the loan has not been returned,
    /// regardless of whether <see cref="Status"/> has been materialised to
    /// <see cref="LoanStatus.Overdue"/> yet. This is the read-time half of
    /// ADR-0011, and what business rule 2 (blocking new loans) must use -
    /// it cannot depend on a background job having already run.
    /// </summary>
    /// <remarks>
    /// Compares calendar dates (UTC), not exact instants - a loan due
    /// "7/9" is not overdue until "8/9" begins, regardless of what
    /// time-of-day <see cref="DueDate"/> happens to carry. Comparing exact
    /// instants would make a loan due at, say, 00:00 on its due date
    /// overdue for almost the entire day it is actually still due - see the
    /// 2026-09-07 addendum to ADR-0011.
    /// </remarks>
    public bool IsOverdueNow(IClock clock) =>
        Status == LoanStatus.Overdue || (Status == LoanStatus.Active && clock.UtcNow.UtcDateTime.Date > DueDate.UtcDateTime.Date);

    /// <summary>
    /// The write-time half of ADR-0011: materialises <see cref="Status"/> to
    /// <see cref="LoanStatus.Overdue"/> if the due date has passed. Intended
    /// to be called by a recurring background job, not built in this change.
    /// </summary>
    public void RefreshOverdueStatus(IClock clock)
    {
        if (IsOverdueNow(clock))
        {
            Status = LoanStatus.Overdue;
            Touch(clock, staffId: null);
        }
    }

    /// <summary>
    /// Equipment handed back. Computes <see cref="DaysLate"/> against the due
    /// date - by calendar date, same as <see cref="IsOverdueNow"/>, so a
    /// return on the due date itself is never "late".
    /// </summary>
    public void Return(IClock clock, Guid? staffId)
    {
        RequireOpen();

        DateTimeOffset now = clock.UtcNow;
        ReturnedAt = now;
        DaysLate = now.UtcDateTime.Date > DueDate.UtcDateTime.Date ? (now.UtcDateTime.Date - DueDate.UtcDateTime.Date).Days : 0;
        Status = LoanStatus.Returned;
        Touch(clock, staffId);
    }

    /// <summary>
    /// Corrects the facts of an open loan that was registered wrongly. Only
    /// Active and Overdue loans may be corrected - RequireOpen() refuses a
    /// Returned or Lost one, because DaysLate and the borrower's late-return
    /// counter were already computed from the old values and cannot be
    /// recomputed from here. See ADR-0025.
    ///
    /// Status is reset to Active and then re-derived by the caller through
    /// RefreshOverdueStatus, so extending a due date past today moves an
    /// Overdue loan back to Active rather than leaving it stale.
    /// </summary>
    public void Correct(
        Guid borrowerId,
        Guid equipmentId,
        DateTimeOffset startedAt,
        DateTimeOffset dueDate,
        IClock clock,
        Guid? staffId)
    {
        RequireOpen();

        if (dueDate <= startedAt)
        {
            throw new ArgumentException("Loan due date must be after the start date.", nameof(dueDate));
        }

        BorrowerId = borrowerId;
        EquipmentId = equipmentId;

        // Same UTC normalisation as the constructor - see the comment there.
        StartedAt = startedAt.ToUniversalTime();
        DueDate = dueDate.ToUniversalTime();
        Status = LoanStatus.Active;
        Touch(clock, staffId);
    }

    /// <summary>The equipment was confirmed lost or destroyed while on loan. This is a terminal state.</summary>
    public void MarkLost(IClock clock, Guid? staffId)
    {
        RequireOpen();
        Status = LoanStatus.Lost;
        Touch(clock, staffId);
    }

    private void RequireOpen()
    {
        if (Status is LoanStatus.Returned or LoanStatus.Lost)
        {
            throw new InvalidOperationException($"Cannot change a loan that is already {Status}.");
        }
    }
}
