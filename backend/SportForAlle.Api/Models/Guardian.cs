using System.Text.RegularExpressions;
using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// The adult responsible for one or more borrowers. Contacted when a loan
/// becomes overdue.
/// </summary>
public partial class Guardian : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int EmailMaxLength = 320;
    public const int PhoneMaxLength = 30;

    /// <summary>Placeholder values written when a guardian is anonymised. Kept valid so existing validation still holds.</summary>
    public const string AnonymisedName = "Anonymisert foresatt";
    public const string AnonymisedEmail = "anonymisert@ugyldig.invalid";
    public const string AnonymisedPhone = "00000000";

    private readonly List<Borrower> _borrowers = [];

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Guardian()
    {
    }

    /// <param name="identityVerified">
    /// True if staff visually confirmed this guardian's ID (name and date of
    /// birth) in person at registration. This is the project's alternative to
    /// storing a fødselsnummer - see docs/09-lover-og-regler.md - so it
    /// records only that a check happened, never the document or number
    /// itself. Optional: registration is not blocked by it, see business
    /// discussion in docs/09-lover-og-regler.md.
    /// </param>
    public Guardian(
        string name, string email, string phone, IClock clock, Guid? createdByStaffId, bool identityVerified = false)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
        Email = ValidateEmail(email);
        Phone = ValidatePhone(phone);
        IdentityVerifiedAt = identityVerified ? clock.UtcNow : null;
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    /// <summary>Null if staff have not yet visually confirmed this guardian's ID in person.</summary>
    public DateTimeOffset? IdentityVerifiedAt { get; private set; }

    /// <summary>Archived guardians are hidden from the working lists but fully restorable. See ADR-0026.</summary>
    public DateTimeOffset? ArchivedAt { get; private set; }

    /// <summary>Set once the contact details have been stripped. Irreversible.</summary>
    public DateTimeOffset? AnonymisedAt { get; private set; }

    public bool IsArchived => ArchivedAt is not null;

    public bool IsAnonymised => AnonymisedAt is not null;

    public IReadOnlyCollection<Borrower> Borrowers => _borrowers;

    public void Archive(IClock clock, Guid? staffId)
    {
        if (IsArchived)
        {
            throw new InvalidOperationException("This guardian is already archived.");
        }

        ArchivedAt = clock.UtcNow;
        Touch(clock, staffId);
    }

    public void Restore(IClock clock, Guid? staffId)
    {
        if (IsAnonymised)
        {
            throw new InvalidOperationException("An anonymised guardian cannot be restored.");
        }

        if (!IsArchived)
        {
            throw new InvalidOperationException("This guardian is not archived.");
        }

        ArchivedAt = null;
        Touch(clock, staffId);
    }

    /// <summary>
    /// Strips name, email and phone. The row survives so the borrowers that
    /// reference it stay valid, but nothing identifying remains - see
    /// docs/09-lover-og-regler.md and ADR-0026. The placeholder contact values
    /// are syntactically valid so existing validation and the email column
    /// keep working.
    /// </summary>
    public void Anonymise(IClock clock, Guid? staffId)
    {
        if (IsAnonymised)
        {
            throw new InvalidOperationException("This guardian is already anonymised.");
        }

        Name = AnonymisedName;
        Email = AnonymisedEmail;
        Phone = AnonymisedPhone;
        IdentityVerifiedAt = null;
        AnonymisedAt = clock.UtcNow;
        ArchivedAt ??= clock.UtcNow;
        Touch(clock, staffId);
    }

    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        Touch(clock, staffId);
    }

    public void ChangeEmail(string email, IClock clock, Guid? staffId)
    {
        Email = ValidateEmail(email);
        Touch(clock, staffId);
    }

    public void ChangePhone(string phone, IClock clock, Guid? staffId)
    {
        Phone = ValidatePhone(phone);
        Touch(clock, staffId);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Guardian name cannot be empty.", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Guardian name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return trimmed;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Guardian email cannot be empty.", nameof(email));
        }

        string trimmed = email.Trim();

        if (trimmed.Length > EmailMaxLength || !EmailPattern().IsMatch(trimmed))
        {
            throw new ArgumentException("Guardian email is not a valid email address.", nameof(email));
        }

        return trimmed;
    }

    private static string ValidatePhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new ArgumentException("Guardian phone cannot be empty.", nameof(phone));
        }

        string trimmed = phone.Trim();

        if (trimmed.Length > PhoneMaxLength)
        {
            throw new ArgumentException($"Guardian phone cannot exceed {PhoneMaxLength} characters.", nameof(phone));
        }

        return trimmed;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
