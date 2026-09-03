using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class BorrowerConfiguration : IEntityTypeConfiguration<Borrower>
{
    public void Configure(EntityTypeBuilder<Borrower> builder)
    {
        builder.ToTable("Borrowers");

        builder.Property(borrower => borrower.Name)
            .IsRequired()
            .HasMaxLength(Borrower.NameMaxLength);

        builder.Property(borrower => borrower.DateOfBirth)
            .IsRequired();

        builder.Property(borrower => borrower.LateReturnCount)
            .IsRequired();

        builder.Property(borrower => borrower.IsUnreliable)
            .IsRequired();

        builder.Property(borrower => borrower.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // A child cannot exist without a guardian - see business rule 1 in
        // docs/03-domenemodell.md. Restrict, not cascade: deleting a guardian
        // must not silently delete the children linked to them.
        builder.HasOne<Guardian>()
            .WithMany(guardian => guardian.Borrowers)
            .HasForeignKey(borrower => borrower.GuardianId)
            .OnDelete(DeleteBehavior.Restrict);

        // Bans have no meaning without the borrower they describe.
        builder.HasMany(borrower => borrower.Bans)
            .WithOne()
            .HasForeignKey(ban => ban.BorrowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
