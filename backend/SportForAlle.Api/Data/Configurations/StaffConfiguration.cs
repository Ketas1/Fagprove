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

        // Nullable on purpose - see Staff.NormaliseOptional. Existing rows
        // predate these columns and legitimately have nothing recorded.
        builder.Property(staff => staff.JobTitle)
            .HasMaxLength(Staff.JobTitleMaxLength);

        builder.Property(staff => staff.Email)
            .HasMaxLength(Staff.EmailMaxLength);

        builder.Property(staff => staff.Phone)
            .HasMaxLength(Staff.PhoneMaxLength);

        builder.Property(staff => staff.Auth0UserId)
            .HasMaxLength(Staff.Auth0UserIdMaxLength);

        builder.HasIndex(staff => staff.Auth0UserId)
            .IsUnique();

        builder.ConfigureAuditableEntity();
    }
}
