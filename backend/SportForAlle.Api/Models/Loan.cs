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

        DueDate = dueDate;
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
    public bool IsOverdueNow(IClock clock) =>
        Status == LoanStatus.Overdue || (Status == LoanStatus.Active && clock.UtcNow > DueDate);

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

    /// <summary>Equipment handed back. Computes <see cref="DaysLate"/> against the due date.</summary>
    public void Return(IClock clock, Guid? staffId)
    {
        RequireOpen();

        DateTimeOffset now = clock.UtcNow;
        ReturnedAt = now;
        DaysLate = now > DueDate ? (now.Date - DueDate.Date).Days : 0;
        Status = LoanStatus.Returned;
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
