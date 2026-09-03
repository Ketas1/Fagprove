using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class EquipmentCategoryConfiguration : IEntityTypeConfiguration<EquipmentCategory>
{
    public void Configure(EntityTypeBuilder<EquipmentCategory> builder)
    {
        builder.ToTable("EquipmentCategories");

        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(EquipmentCategory.NameMaxLength);

        builder.HasIndex(category => category.Name)
            .IsUnique();
    }
}
