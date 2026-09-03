using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// Groups equipment for reporting, for example ski, bicycles or skates.
/// </summary>
/// <remarks>
/// Entities in this project own their own state. Setters are private and every
/// change goes through a method that validates first, so an invalid instance
/// cannot be constructed or assigned into existence from elsewhere in the code.
/// </remarks>
public class EquipmentCategory : AuditableEntity
{
    public const int NameMaxLength = 100;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private EquipmentCategory()
    {
    }

    public EquipmentCategory(string name, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Changes the category name, rejecting anything blank or too long.</summary>
    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        Touch(clock, staffId);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException(
                $"Category name cannot exceed {NameMaxLength} characters.",
                nameof(name));
        }

        return trimmed;
    }
}
