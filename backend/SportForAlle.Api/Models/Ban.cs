using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// One period during which a borrower was not allowed to borrow. Created by
/// <see cref="Borrower.Ban"/> and closed by <see cref="Borrower.LiftBan"/> -
/// never constructed or changed directly, so a ban cannot exist without a
/// reason or be lifted twice.
/// </summary>
public class Ban : AuditableEntity
{
    public const int ReasonMaxLength = 2000;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Ban()
    {
    }

    internal Ban(Guid borrowerId, string reason, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Ban reason cannot be empty.", nameof(reason));
        }

        string trimmed = reason.Trim();

        if (trimmed.Length > ReasonMaxLength)
        {
            throw new ArgumentException($"Ban reason cannot exceed {ReasonMaxLength} characters.", nameof(reason));
        }

        BorrowerId = borrowerId;
        Reason = trimmed;
    }

    public Guid BorrowerId { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    /// <summary>Null while the ban is still in effect.</summary>
    public DateTimeOffset? LiftedAt { get; private set; }

    public Guid? LiftedByStaffId { get; private set; }

    /// <summary>Set once the fee has been recorded as paid in the shop. Required before a ban can be lifted.</summary>
    public DateTimeOffset? FeePaidAt { get; private set; }

    public bool IsActive => LiftedAt is null;

    /// <summary>Records the fee payment that unblocks lifting this ban. See business rule 8 in docs/03-domenemodell.md.</summary>
    internal void RecordFeePaid(IClock clock)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("Cannot record a fee payment for a ban that has already been lifted.");
        }

        FeePaidAt = clock.UtcNow;
    }

    internal void Lift(IClock clock, Guid? staffId)
    {
        if (!IsActive)
        {
            throw new InvalidOperationException("This ban has already been lifted.");
        }

        if (FeePaidAt is null)
        {
            throw new InvalidOperationException("Cannot lift a ban before the fee has been recorded as paid.");
        }

        LiftedAt = clock.UtcNow;
        LiftedByStaffId = staffId;
    }
}
