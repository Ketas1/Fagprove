using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff");

        builder.Property(staff => staff.Name)
            .IsRequired()
            .HasMaxLength(Staff.NameMaxLength);

        builder.Property(staff => staff.Auth0UserId)
            .HasMaxLength(Staff.Auth0UserIdMaxLength);

        builder.HasIndex(staff => staff.Auth0UserId)
            .IsUnique();

        builder.ConfigureAuditableEntity();
    }
}
