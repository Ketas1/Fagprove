using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Dtos.Guardians;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Borrowers;

/// <summary>
/// Exactly one of <see cref="GuardianId"/> (link an existing guardian, for
/// example a second child of the same parent) and <see cref="NewGuardian"/>
/// (create one in the same request) must be given - enforced in
/// <c>BorrowerService</c>, not by an attribute here, since it needs to
/// compare two properties against each other.
/// </summary>
public record CreateBorrowerRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(Borrower.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "Fødselsdato er påkrevd.")]
    public required DateOnly DateOfBirth { get; init; }

    public Guid? GuardianId { get; init; }

    public CreateGuardianRequest? NewGuardian { get; init; }
}
