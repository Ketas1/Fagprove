using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Borrowers;

/// <summary>
/// Name and date of birth. <see cref="Borrower.GuardianId"/> stays immutable -
/// moving a child to a different guardian is a different operation from
/// correcting a typo, and has no endpoint. Changing the date of birth
/// recalculates historical age-group reports; see ADR-0024.
/// </summary>
public record UpdateBorrowerRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(Borrower.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Fødselsdato er påkrevd.")]
    public required DateOnly DateOfBirth { get; init; }
}
