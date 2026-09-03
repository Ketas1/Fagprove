using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// The child who borrows equipment. Has no login of their own - a guardian is
/// registered first, and every borrower is linked to exactly one.
/// </summary>
public class Borrower : AuditableEntity
{
    public const int NameMaxLength = 200;

    private readonly List<Ban> _bans = [];

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Borrower()
    {
    }

    public Borrower(string name, DateOnly dateOfBirth, Guid guardianId, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
        DateOfBirth = ValidateDateOfBirth(dateOfBirth, clock);
        GuardianId = guardianId;
    }

    public string Name { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public Guid GuardianId { get; private set; }

    /// <summary>How many times this borrower has returned equipment after the due date.</summary>
    public int LateReturnCount { get; private set; }

    /// <summary>
    /// A warning flag, not a status: an unreliable borrower may still borrow.
    /// Do not confuse with <see cref="Status"/>.
    /// </summary>
    public bool IsUnreliable { get; private set; }

    public BorrowerStatus Status { get; private set; } = BorrowerStatus.Active;

    public IReadOnlyCollection<Ban> Bans => _bans;

    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        Touch(clock, staffId);
    }

    /// <summary>Called when equipment is returned after the due date. Increments the counter and flags the borrower.</summary>
    public void RecordLateReturn(IClock clock, Guid? staffId)
    {
        LateReturnCount++;
        IsUnreliable = true;
        Touch(clock, staffId);
    }

    /// <summary>
    /// Bans this borrower, recording the reason. See business rule 2 in
    /// docs/03-domenemodell.md - a banned borrower cannot register a new loan.
    /// </summary>
    public void Ban(string reason, IClock clock, Guid? staffId)
    {
        if (Status == BorrowerStatus.Banned)
        {
            throw new InvalidOperationException("This borrower is already banned.");
        }

        _bans.Add(new Ban(Id, reason, clock, staffId));
        Status = BorrowerStatus.Banned;
        Touch(clock, staffId);
    }

    /// <summary>Records that the fee for the current ban has been paid in the shop.</summary>
    public void RecordBanFeePaid(IClock clock, Guid? staffId)
    {
        CurrentBan().RecordFeePaid(clock);
        Touch(clock, staffId);
    }

    /// <summary>
    /// Lifts the current ban. Requires the fee to already be recorded as paid -
    /// see business rule 8 in docs/03-domenemodell.md.
    /// </summary>
    public void LiftBan(IClock clock, Guid? staffId)
    {
        CurrentBan().Lift(clock, staffId);
        Status = BorrowerStatus.Active;
        Touch(clock, staffId);
    }

    private Ban CurrentBan()
    {
        if (Status != BorrowerStatus.Banned)
        {
            throw new InvalidOperationException("This borrower is not currently banned.");
        }

        return _bans.SingleOrDefault(ban => ban.IsActive)
            ?? throw new InvalidOperationException("This borrower is banned, but has no active ban record.");
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Borrower name cannot be empty.", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Borrower name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return trimmed;
    }

    private static DateOnly ValidateDateOfBirth(DateOnly dateOfBirth, IClock clock)
    {
        if (dateOfBirth > DateOnly.FromDateTime(clock.UtcNow.UtcDateTime))
        {
            throw new ArgumentException("Borrower date of birth cannot be in the future.", nameof(dateOfBirth));
        }

        return dateOfBirth;
    }
}
