using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Borrowers;

/// <summary>
/// Rename only - <see cref="Borrower"/> has no entity method to change
/// <see cref="Borrower.DateOfBirth"/> or <see cref="Borrower.GuardianId"/>
/// after creation.
/// </summary>
public record UpdateBorrowerRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(Borrower.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }
}
