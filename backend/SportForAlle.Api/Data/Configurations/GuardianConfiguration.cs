using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class GuardianConfiguration : IEntityTypeConfiguration<Guardian>
{
    public void Configure(EntityTypeBuilder<Guardian> builder)
    {
        builder.ToTable("Guardians");

        builder.Property(guardian => guardian.Name)
            .IsRequired()
            .HasMaxLength(Guardian.NameMaxLength);

        builder.Property(guardian => guardian.Email)
            .IsRequired()
            .HasMaxLength(Guardian.EmailMaxLength);

        builder.Property(guardian => guardian.Phone)
            .IsRequired()
            .HasMaxLength(Guardian.PhoneMaxLength);

        builder.ConfigureAuditableEntity();
    }
}
