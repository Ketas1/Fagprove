using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// One attempt to reach a guardian about an overdue loan. Logged with method
/// and outcome so staff can see whether a guardian has already been
/// contacted - see business rule 6 in docs/03-domenemodell.md. The attempt
/// time is <see cref="AuditableEntity.CreatedAt"/>; there is no separate field.
/// </summary>
public class ContactAttempt : AuditableEntity
{
    public const int OutcomeMaxLength = 1000;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private ContactAttempt()
    {
    }

    public ContactAttempt(Guid loanId, ContactMethod method, string outcome, IClock clock, Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        LoanId = loanId;
        Method = method;

        if (string.IsNullOrWhiteSpace(outcome))
        {
            throw new ArgumentException("Contact attempt outcome cannot be empty.", nameof(outcome));
        }

        string trimmed = outcome.Trim();

        if (trimmed.Length > OutcomeMaxLength)
        {
            throw new ArgumentException(
                $"Contact attempt outcome cannot exceed {OutcomeMaxLength} characters.",
                nameof(outcome));
        }

        Outcome = trimmed;
    }

    public Guid LoanId { get; private set; }

    public ContactMethod Method { get; private set; }

    public string Outcome { get; private set; } = string.Empty;
}
