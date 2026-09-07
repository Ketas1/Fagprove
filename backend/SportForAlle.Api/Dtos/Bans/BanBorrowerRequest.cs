using System.ComponentModel.DataAnnotations;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Bans;

public record BanBorrowerRequest
{
    [Required(ErrorMessage = "Årsak er påkrevd.")]
    [MaxLength(Ban.ReasonMaxLength, ErrorMessage = "Årsak er for lang.")]
    public required string Reason { get; init; }
}
