using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class ContactAttemptConfiguration : IEntityTypeConfiguration<ContactAttempt>
{
    public void Configure(EntityTypeBuilder<ContactAttempt> builder)
    {
        builder.ToTable("ContactAttempts");

        builder.Property(attempt => attempt.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(attempt => attempt.Outcome)
            .IsRequired()
            .HasMaxLength(ContactAttempt.OutcomeMaxLength);

        // A contact attempt has no meaning without the loan it followed up on.
        builder.HasOne<Loan>()
            .WithMany()
            .HasForeignKey(attempt => attempt.LoanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
