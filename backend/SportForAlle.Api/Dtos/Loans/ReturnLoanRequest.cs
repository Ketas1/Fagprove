using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Loans;

public record ReturnLoanRequest
{
    [Required(ErrorMessage = "Tilstand er påkrevd.")]
    public required EquipmentCondition Condition { get; init; }
}
