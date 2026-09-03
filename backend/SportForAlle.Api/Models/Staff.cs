using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// An employee of the shop. Linked to an Auth0 user once authentication is
/// wired up; until then <see cref="Auth0UserId"/> stays empty.
/// </summary>
public class Staff : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int Auth0UserIdMaxLength = 100;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Staff()
    {
    }

    public Staff(string name, IClock clock)
        : base(clock, createdByStaffId: null)
    {
        Name = ValidateName(name);
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>The Auth0 subject identifier (the "sub" claim) for this staff member.</summary>
    public string? Auth0UserId { get; private set; }

    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
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
}
