using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Guardians;

public record UpdateGuardianRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(Guardian.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [Required(ErrorMessage = "E-post er påkrevd.")]
    [EmailAddress(ErrorMessage = "E-post har ugyldig format.")]
    [MaxLength(Guardian.EmailMaxLength, ErrorMessage = "E-post er for lang.")]
    public required string Email { get; init; }

    [Required(ErrorMessage = "Telefon er påkrevd.")]
    [MaxLength(Guardian.PhoneMaxLength, ErrorMessage = "Telefon er for langt.")]
    public required string Phone { get; init; }
}
