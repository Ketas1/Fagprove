using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

/// <summary>
/// Entities validate their own state, so these run with no database, no HTTP
/// and no dependency injection.
/// </summary>
public class EquipmentCategoryTests
{
    private static readonly FakeClock _clock = new();

    [Fact]
    public void Constructor_trims_surrounding_whitespace()
    {
        EquipmentCategory category = new("  Ski  ", _clock, createdByStaffId: null);

        Assert.Equal("Ski", category.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_name(string name)
    {
        Assert.Throws<ArgumentException>(() => new EquipmentCategory(name, _clock, createdByStaffId: null));
    }

    [Fact]
    public void Constructor_rejects_a_name_that_is_too_long()
    {
        string tooLong = new('a', EquipmentCategory.NameMaxLength + 1);

        Assert.Throws<ArgumentException>(() => new EquipmentCategory(tooLong, _clock, createdByStaffId: null));
    }

    [Fact]
    public void Constructor_records_who_created_it_and_when()
    {
        Guid staffId = Guid.NewGuid();
        EquipmentCategory category = new("Ski", _clock, staffId);

        Assert.Equal(_clock.UtcNow, category.CreatedAt);
        Assert.Equal(staffId, category.CreatedByStaffId);
        Assert.Null(category.UpdatedAt);
    }

    [Fact]
    public void Rename_replaces_the_name()
    {
        EquipmentCategory category = new("Ski", _clock, createdByStaffId: null);

        category.Rename("Sykkel", _clock, Guid.NewGuid());

        Assert.Equal("Sykkel", category.Name);
    }

    [Fact]
    public void Rename_records_who_changed_it_and_when()
    {
        EquipmentCategory category = new("Ski", _clock, createdByStaffId: null);
        FakeClock laterClock = new(_clock.UtcNow.AddDays(1));
        Guid staffId = Guid.NewGuid();

        category.Rename("Sykkel", laterClock, staffId);

        Assert.Equal(laterClock.UtcNow, category.UpdatedAt);
        Assert.Equal(staffId, category.UpdatedByStaffId);
    }

    [Fact]
    public void Rename_rejects_a_blank_name_and_leaves_the_original_intact()
    {
        EquipmentCategory category = new("Ski", _clock, createdByStaffId: null);

        Assert.Throws<ArgumentException>(() => category.Rename("  ", _clock, staffId: null));
        Assert.Equal("Ski", category.Name);
    }

    [Fact]
    public void Constructor_defaults_to_a_top_level_category()
    {
        EquipmentCategory category = new("Ski", _clock, createdByStaffId: null);

        Assert.Null(category.ParentCategoryId);
    }

    [Fact]
    public void Constructor_accepts_a_parent_category()
    {
        Guid parentId = Guid.NewGuid();
        EquipmentCategory category = new("Slalåmski", _clock, createdByStaffId: null, parentId);

        Assert.Equal(parentId, category.ParentCategoryId);
    }
}
