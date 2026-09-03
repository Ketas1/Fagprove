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

    private readonly List<Borrower> _borrowers = [];

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Guardian()
    {
    }

    public Guardian(string name, string email, string phone, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
        Email = ValidateEmail(email);
        Phone = ValidatePhone(phone);
    }

    public string Name { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string Phone { get; private set; } = string.Empty;

    public IReadOnlyCollection<Borrower> Borrowers => _borrowers;

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
