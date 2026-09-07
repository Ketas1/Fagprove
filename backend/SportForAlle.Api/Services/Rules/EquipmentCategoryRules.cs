using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services.Rules;

public static class EquipmentCategoryRules
{
    /// <summary>
    /// A category can only be deleted once it is empty - no subcategories and
    /// no equipment directly in it - the same way a directory refuses to be
    /// removed while it still has something in it. Deletion never cascades.
    /// </summary>
    public static void EnsureCanBeDeleted(bool hasSubcategories, bool hasEquipment)
    {
        if (hasSubcategories)
        {
            throw new DomainConflictException(
                "CategoryHasSubcategories", "Kategorien har underkategorier og kan ikke slettes.");
        }

        if (hasEquipment)
        {
            throw new DomainConflictException(
                "CategoryHasEquipment", "Kategorien har utstyr og kan ikke slettes.");
        }
    }
}
