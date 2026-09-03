using SportForAlle.Api.Models;

namespace SportForAlle.Tests.Models;

/// <summary>
/// Entities validate their own state, so these run with no database, no HTTP
/// and no dependency injection.
/// </summary>
public class EquipmentCategoryTests
{
    [Fact]
    public void Constructor_trims_surrounding_whitespace()
    {
        EquipmentCategory category = new("  Ski  ");

        Assert.Equal("Ski", category.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_name(string name)
    {
        Assert.Throws<ArgumentException>(() => new EquipmentCategory(name));
    }

    [Fact]
    public void Constructor_rejects_a_name_that_is_too_long()
    {
        string tooLong = new('a', EquipmentCategory.NameMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new EquipmentCategory(tooLong));
    }

    [Fact]
    public void Rename_replaces_the_name()
    {
        EquipmentCategory category = new("Ski");

        category.Rename("Sykkel");

        Assert.Equal("Sykkel", category.Name);
    }

    [Fact]
    public void Rename_rejects_a_blank_name_and_leaves_the_original_intact()
    {
        EquipmentCategory category = new("Ski");

        Assert.Throws<ArgumentException>(() => category.Rename("  "));
        Assert.Equal("Ski", category.Name);
    }
}
