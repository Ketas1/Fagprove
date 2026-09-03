using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment");

        builder.Property(equipment => equipment.Name)
            .IsRequired()
            .HasMaxLength(Equipment.NameMaxLength);

        builder.Property(equipment => equipment.SerialNumber)
            .IsRequired()
            .HasMaxLength(Equipment.SerialNumberMaxLength);

        builder.HasIndex(equipment => equipment.SerialNumber)
            .IsUnique();

        builder.Property(equipment => equipment.Condition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(equipment => equipment.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Recategorising equipment must not be blocked by history, but a
        // category with equipment attached cannot be deleted out from under it.
        builder.HasOne<EquipmentCategory>()
            .WithMany()
            .HasForeignKey(equipment => equipment.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditableEntity();
    }
}
