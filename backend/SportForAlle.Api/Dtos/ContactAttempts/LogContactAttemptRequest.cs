using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.ContactAttempts;

public record LogContactAttemptRequest
{
    [Required(ErrorMessage = "Kontaktmetode er påkrevd.")]
    public required ContactMethod Method { get; init; }

    [Required(ErrorMessage = "Resultat er påkrevd.")]
    [MaxLength(ContactAttempt.OutcomeMaxLength, ErrorMessage = "Resultatet er for langt.")]
    public required string Outcome { get; init; }
}
