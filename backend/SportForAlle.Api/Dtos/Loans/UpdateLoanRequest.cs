using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Loans;

/// <summary>
/// Corrects an open loan registered with the wrong facts. Status is absent on
/// purpose: it follows from the due date and from the return/lost transitions,
/// never from an edit form. A Returned or Lost loan is refused outright - see
/// LoanRules.EnsureLoanCanBeCorrected and ADR-0025.
/// </summary>
public record UpdateLoanRequest
{
    [Required(ErrorMessage = "Låntaker er påkrevd.")]
    public required Guid BorrowerId { get; init; }

    [Required(ErrorMessage = "Utstyr er påkrevd.")]
    public required Guid EquipmentId { get; init; }

    [Required(ErrorMessage = "Startdato er påkrevd.")]
    public required DateTimeOffset StartedAt { get; init; }

    [Required(ErrorMessage = "Forventet returdato er påkrevd.")]
    public required DateTimeOffset DueDate { get; init; }
}
