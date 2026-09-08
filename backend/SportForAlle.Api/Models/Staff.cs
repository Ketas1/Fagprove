using System.Text.RegularExpressions;
using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// An employee of the shop. Linked to an Auth0 user once authentication is
/// wired up; until then <see cref="Auth0UserId"/> stays empty.
/// </summary>
/// <remarks>
/// <see cref="JobTitle"/> is the employee's position in the shop
/// ("Butikkleder"), free text and purely informational. It is deliberately
/// <em>not</em> the authorisation role - that comes from Auth0, see
/// docs/adr/0019-staff-auth0-mapping.md. Nothing in the system reads this
/// field to decide what a user is allowed to do.
///
/// No home address is stored. The lending workflow has no use for one, and
/// GDPR data minimisation applies to employees as much as to the children -
/// see docs/09-lover-og-regler.md.
/// </remarks>
public partial class Staff : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int Auth0UserIdMaxLength = 100;
    public const int JobTitleMaxLength = 100;
    public const int EmailMaxLength = 320;
    public const int PhoneMaxLength = 30;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Staff()
    {
    }

    public Staff(string name, IClock clock, string? jobTitle = null, string? email = null, string? phone = null)
        : base(clock, createdByStaffId: null)
    {
        Name = ValidateName(name);
        JobTitle = ValidateJobTitle(jobTitle);
        Email = ValidateEmail(email);
        Phone = ValidatePhone(phone);
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Position in the shop, e.g. "Butikkleder". Informational only - never used for authorisation.</summary>
    public string? JobTitle { get; private set; }

    /// <summary>Work contact details. Null means "not recorded", which is a valid state.</summary>
    public string? Email { get; private set; }

    public string? Phone { get; private set; }

    /// <summary>The Auth0 subject identifier (the "sub" claim) for this staff member.</summary>
    public string? Auth0UserId { get; private set; }

    public void Update(string name, string? jobTitle, string? email, string? phone, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        JobTitle = ValidateJobTitle(jobTitle);
        Email = ValidateEmail(email);
        Phone = ValidatePhone(phone);
        Touch(clock, staffId);
    }

    public void LinkAuth0User(string auth0UserId, IClock clock, Guid? staffId)
    {
        if (string.IsNullOrWhiteSpace(auth0UserId))
        {
            throw new ArgumentException("Auth0 user id cannot be empty.", nameof(auth0UserId));
        }

        string trimmed = auth0UserId.Trim();

        if (trimmed.Length > Auth0UserIdMaxLength)
        {
            throw new ArgumentException(
                $"Auth0 user id cannot exceed {Auth0UserIdMaxLength} characters.",
                nameof(auth0UserId));
        }

        Auth0UserId = trimmed;
        Touch(clock, staffId);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Staff name cannot be empty.", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Staff name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return trimmed;
    }

    private static string? ValidateJobTitle(string? jobTitle) =>
        NormaliseOptional(jobTitle, JobTitleMaxLength, nameof(jobTitle), "Staff job title");

    private static string? ValidatePhone(string? phone) =>
        NormaliseOptional(phone, PhoneMaxLength, nameof(phone), "Staff phone");

    private static string? ValidateEmail(string? email)
    {
        string? trimmed = NormaliseOptional(email, EmailMaxLength, nameof(email), "Staff email");

        if (trimmed is not null && !EmailPattern().IsMatch(trimmed))
        {
            throw new ArgumentException("Staff email is not a valid email address.", nameof(email));
        }

        return trimmed;
    }

    /// <summary>
    /// The three added fields are optional, so an empty or whitespace-only
    /// value means "not recorded" and normalises to null - otherwise clearing
    /// a field in the UI would store an empty string that reads as recorded.
    /// </summary>
    private static string? NormaliseOptional(string? value, int maxLength, string paramName, string label)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();

        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"{label} cannot exceed {maxLength} characters.", paramName);
        }

        return trimmed;
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();
}
