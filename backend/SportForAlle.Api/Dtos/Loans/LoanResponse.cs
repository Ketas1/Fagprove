using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Loans;

/// <summary>
/// <paramref name="ContactAttemptCount"/> and <paramref name="LastContactedAt"/>
/// let a loan list show whether a guardian has already been contacted
/// (business rule 5 in docs/03-domenemodell.md) without the client having to
/// fetch <c>/api/loans/{id}/contact-attempts</c> per row.
/// <paramref name="LastContactedAt"/> is null exactly when the count is zero.
/// </summary>
public record LoanResponse(
    Guid Id,
    Guid BorrowerId,
    string BorrowerName,
    Guid EquipmentId,
    string EquipmentName,
    DateTimeOffset StartedAt,
    DateTimeOffset DueDate,
    DateTimeOffset? ReturnedAt,
    int? DaysLate,
    LoanStatus Status,
    int ContactAttemptCount,
    DateTimeOffset? LastContactedAt,
    Guid? CreatedByStaffId,
    Guid? UpdatedByStaffId);
