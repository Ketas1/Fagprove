using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Tests.Services.Rules;

public class EquipmentCategoryRulesTests
{
    [Fact]
    public void EnsureCanBeDeleted_allows_an_empty_category()
    {
        EquipmentCategoryRules.EnsureCanBeDeleted(hasSubcategories: false, hasEquipment: false);
    }

    [Fact]
    public void EnsureCanBeDeleted_rejects_a_category_with_subcategories()
    {
        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => EquipmentCategoryRules.EnsureCanBeDeleted(hasSubcategories: true, hasEquipment: false));

        Assert.Equal("CategoryHasSubcategories", exception.Reason);
    }

    [Fact]
    public void EnsureCanBeDeleted_rejects_a_category_with_equipment()
    {
        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => EquipmentCategoryRules.EnsureCanBeDeleted(hasSubcategories: false, hasEquipment: true));

        Assert.Equal("CategoryHasEquipment", exception.Reason);
    }

    [Fact]
    public void EnsureCanBeDeleted_rejects_a_category_with_both()
    {
        Assert.Throws<DomainConflictException>(
            () => EquipmentCategoryRules.EnsureCanBeDeleted(hasSubcategories: true, hasEquipment: true));
    }
}
