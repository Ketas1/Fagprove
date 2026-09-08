using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// The child who borrows equipment. Has no login of their own - a guardian is
/// registered first, and every borrower is linked to exactly one.
/// </summary>
public class Borrower : AuditableEntity
{
    public const int NameMaxLength = 200;

    /// <summary>Replaces the name once a borrower is anonymised. Norwegian, because it is shown in the UI.</summary>
    public const string AnonymisedName = "Anonymisert låntaker";

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

    /// <summary>
    /// Set while the borrower is archived - hidden from the working lists but
    /// fully restorable. An operational state (aged out, moved away), not a
    /// deletion. See ADR-0026.
    /// </summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>
    /// Set once the personal data has been stripped. Irreversible, and the end
    /// state archiving must eventually reach - GDPR article 5 (1) e does not
    /// allow keeping a child's name indefinitely just because there is loan
    /// history. See docs/09-lover-og-regler.md.
    /// </summary>
    public DateTimeOffset? AnonymisedAt { get; private set; }

    public bool IsArchived => ArchivedAt is not null;

    public bool IsAnonymised => AnonymisedAt is not null;

    public IReadOnlyCollection<Ban> Bans => _bans;

    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        Touch(clock, staffId);
    }

    /// <summary>
    /// Corrects a date of birth that was entered wrongly at registration.
    /// This also changes historical age-group reports: ReportService
    /// .GetAgeGroupsAsync joins Borrowers and reads DateOfBirth live rather
    /// than freezing an age onto the loan, so past periods are recalculated
    /// from the corrected date. That is the intended behaviour - if the date
    /// was wrong, the figures derived from it were wrong too. See ADR-0024.
    /// </summary>
    public void ChangeDateOfBirth(DateOnly dateOfBirth, IClock clock, Guid? staffId)
    {
        DateOfBirth = ValidateDateOfBirth(dateOfBirth, clock);
        Touch(clock, staffId);
    }

    /// <summary>Hides the borrower from the working lists without losing anything. Reversible through <see cref="Restore"/>.</summary>
    public void Archive(IClock clock, Guid? staffId)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("This borrower is already archived.");
        }

        ArchivedAt = clock.UtcNow;
        Touch(clock, staffId);
    }

    public void Restore(IClock clock, Guid? staffId)
    {
        if (IsAnonymised)
        {
            throw new InvalidOperationException("An anonymised borrower cannot be restored.");
        }

        if (!IsArchived)
        {
            throw new InvalidOperationException("This borrower is not archived.");
        }

        ArchivedAt = null;
        Touch(clock, staffId);
    }

    /// <summary>
    /// Strips the identifying data but keeps the row, so the loans that
    /// reference it stay countable in the municipality's reports. This is how
    /// an article 17 erasure request is answered for a child who has loan
    /// history - see docs/09-lover-og-regler.md and ADR-0026.
    ///
    /// <see cref="DateOfBirth"/> is deliberately kept: the age-group report
    /// derives the age at loan time from it, and a date of birth without a
    /// name does not identify anyone on its own. The caller is responsible for
    /// deleting the free-text notes, which can name people.
    /// </summary>
    public void Anonymise(IClock clock, Guid? staffId)
    {
        if (IsAnonymised)
        {
            throw new InvalidOperationException("This borrower is already anonymised.");
        }

        Name = AnonymisedName;
        AnonymisedAt = clock.UtcNow;
        ArchivedAt ??= clock.UtcNow;
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
