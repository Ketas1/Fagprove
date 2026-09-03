using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Data.Configurations;

public class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");

        builder.Property(note => note.Text)
            .IsRequired()
            .HasMaxLength(Note.TextMaxLength);

        // A note is about the borrower and has no meaning without them - see
        // the right-to-erasure discussion in docs/09-lover-og-regler.md.
        builder.HasOne<Borrower>()
            .WithMany()
            .HasForeignKey(note => note.BorrowerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditableEntity();
    }
}
