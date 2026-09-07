using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Notes;

public record CreateNoteRequest
{
    [Required(ErrorMessage = "Tekst er påkrevd.")]
    [MaxLength(Note.TextMaxLength, ErrorMessage = "Teksten er for lang.")]
    public required string Text { get; init; }
}
