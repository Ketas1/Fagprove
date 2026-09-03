using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class LoanConfiguration : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("Loans");

        builder.Property(loan => loan.StartedAt).IsRequired();
        builder.Property(loan => loan.DueDate).IsRequired();

        builder.Property(loan => loan.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        // Loan history is the basis for reporting and must survive deletion
        // of the borrower or equipment it involved - see
        // docs/04-databasedesign.md, "Nøkler og relasjoner".
        builder.HasOne<Borrower>()
            .WithMany()
            .HasForeignKey(loan => loan.BorrowerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Equipment>()
            .WithMany()
            .HasForeignKey(loan => loan.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        // The overdue overview is the system's most-used query.
        builder.HasIndex(loan => new { loan.Status, loan.DueDate });

        // The blocked-loan check (business rule 2) runs on every new loan.
        builder.HasIndex(loan => new { loan.BorrowerId, loan.Status });

        builder.ConfigureAuditableEntity();
    }
}
