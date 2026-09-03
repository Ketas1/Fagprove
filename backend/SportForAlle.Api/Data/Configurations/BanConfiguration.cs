using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class BanConfiguration : IEntityTypeConfiguration<Ban>
{
    public void Configure(EntityTypeBuilder<Ban> builder)
    {
        builder.ToTable("Bans");

        builder.Property(ban => ban.Reason)
            .IsRequired()
            .HasMaxLength(Ban.ReasonMaxLength);

        // The Borrower <-> Ban relationship (including its delete behaviour)
        // is configured from BorrowerConfiguration, which owns the collection.

        builder.HasOne<Staff>()
            .WithMany()
            .HasForeignKey(ban => ban.LiftedByStaffId)
            .OnDelete(DeleteBehavior.SetNull);

        // Enforces at the database level, not just in Borrower.Ban(), that a
        // borrower cannot have more than one ban in effect at a time.
        builder.HasIndex(ban => ban.BorrowerId)
            .IsUnique()
            .HasFilter("\"LiftedAt\" IS NULL");

        builder.ConfigureAuditableEntity();
    }
}
