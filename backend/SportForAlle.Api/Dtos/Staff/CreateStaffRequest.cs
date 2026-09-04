using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Staff;

public record CreateStaffRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Staff.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }
}
