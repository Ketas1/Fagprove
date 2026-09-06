using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// Groups equipment for reporting, for example ski, bicycles or skates.
/// Categories may nest under one another (<see cref="ParentCategoryId"/>) to
/// an arbitrary depth - the depth is not enforced here or anywhere else. That
/// is a deliberate choice: it is a soft, usage-driven convention (staff are
/// not expected to nest more than 2-3 levels for a catalog this size), not a
/// data-integrity rule, so there is nothing to validate. A category may hold
/// both its own equipment and subcategories at the same time - there is no
/// "leaf-only" restriction.
/// </summary>
/// <remarks>
/// Entities in this project own their own state. Setters are private and every
/// change goes through a method that validates first, so an invalid instance
/// cannot be constructed or assigned into existence from elsewhere in the code.
/// A category's parent is only ever set here, at construction - there is no
/// "move" operation, so a cycle (a category becoming its own ancestor) is
/// structurally impossible: a new category can only point at a parent that
/// already exists.
/// </remarks>
public class EquipmentCategory : AuditableEntity
{
    public const int NameMaxLength = 100;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private EquipmentCategory()
    {
    }

    public EquipmentCategory(string name, IClock clock, Guid? createdByStaffId, Guid? parentCategoryId = null)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
        ParentCategoryId = parentCategoryId;
    }

    public string Name { get; private set; } = string.Empty;

    /// <summary>Null for a top-level category.</summary>
    public Guid? ParentCategoryId { get; private set; }

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
