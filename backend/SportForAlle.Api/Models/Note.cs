using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// A free-text note staff write about a borrower - documenting follow-up or
/// the reason for a ban. See the guidance on notes in docs/09-lover-og-regler.md:
/// notes are personal data, and must stay factual, not characterise the child
/// or family.
/// </summary>
public class Note : AuditableEntity
{
    public const int TextMaxLength = 2000;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Note()
    {
    }

    public Note(Guid borrowerId, string text, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        BorrowerId = borrowerId;
        Text = Validate(text);
    }

    public Guid BorrowerId { get; private set; }

    public string Text { get; private set; } = string.Empty;

    public void Rewrite(string text, IClock clock, Guid? staffId)
    {
        Text = Validate(text);
        Touch(clock, staffId);
    }

    private static string Validate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Note text cannot be empty.", nameof(text));
        }

        string trimmed = text.Trim();

        if (trimmed.Length > TextMaxLength)
        {
            throw new ArgumentException($"Note text cannot exceed {TextMaxLength} characters.", nameof(text));
        }

        return trimmed;
    }
}
