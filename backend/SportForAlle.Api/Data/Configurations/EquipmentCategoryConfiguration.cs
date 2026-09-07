using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class EquipmentCategoryConfiguration : IEntityTypeConfiguration<EquipmentCategory>
{
    public void Configure(EntityTypeBuilder<EquipmentCategory> builder)
    {
        builder.ToTable("EquipmentCategories");

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(EquipmentCategory.NameMaxLength);

        // Names are unique among siblings, not globally - two different
        // branches may each have a subcategory called "Diverse". Postgres
        // treats every NULL as distinct in a plain composite unique index, so
        // a single (ParentCategoryId, Name) index would let two *root*
        // categories share a name (both rows have ParentCategoryId = NULL,
        // and NULL <> NULL). Splitting into two filtered indexes closes that
        // gap: one for actual siblings, one for root-level categories.
        builder.HasIndex(category => new { category.ParentCategoryId, category.Name })
            .IsUnique()
            .HasFilter("\"ParentCategoryId\" IS NOT NULL");

        builder.HasIndex(category => category.Name)
            .IsUnique()
            .HasFilter("\"ParentCategoryId\" IS NULL");

        // A category with subcategories cannot be deleted out from under
        // them - see EquipmentCategoryRules.EnsureCanBeDeleted for the
        // service-level check that turns this into a clean 409 first.
        builder.HasOne<EquipmentCategory>()
            .WithMany()
            .HasForeignKey(category => category.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditableEntity();
    }
}
