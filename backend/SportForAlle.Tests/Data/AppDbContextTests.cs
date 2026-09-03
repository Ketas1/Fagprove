using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SportForAlle.Api.Data;
using SportForAlle.Api.Models;

namespace SportForAlle.Tests.Data;

/// <summary>
/// Builds the EF model without touching a database. A broken
/// IEntityTypeConfiguration fails here instead of at the first query.
/// </summary>
public class AppDbContextTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model-validation-only")
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public void Model_builds_and_maps_the_expected_entities()
    {
        using AppDbContext context = CreateContext();

        IEntityType? entityType = context.Model.FindEntityType(typeof(EquipmentCategory));

        Assert.NotNull(entityType);
        Assert.Equal("EquipmentCategories", entityType.GetTableName());
    }

    [Fact]
    public void Configurations_in_the_assembly_are_applied()
    {
        using AppDbContext context = CreateContext();

        IEntityType entityType = context.Model.FindEntityType(typeof(EquipmentCategory))!;
        IProperty name = entityType.FindProperty(nameof(EquipmentCategory.Name))!;

        // These come from EquipmentCategoryConfiguration, so if the assembly scan
        // in OnModelCreating stops working, this fails.
        Assert.False(name.IsNullable);
        Assert.Equal(EquipmentCategory.NameMaxLength, name.GetMaxLength());
    }
}
