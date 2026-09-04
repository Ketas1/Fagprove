using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Loans;

public record CreateLoanRequest
{
    [Required(ErrorMessage = "Låntaker er påkrevd.")]
    public required Guid BorrowerId { get; init; }

    [Required(ErrorMessage = "Utstyr er påkrevd.")]
    public required Guid EquipmentId { get; init; }

    [Required(ErrorMessage = "Forventet returdato er påkrevd.")]
    public required DateTimeOffset DueDate { get; init; }
}
