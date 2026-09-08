using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Staff;

public record CreateStaffRequest
{
    [Required(ErrorMessage = "Navn er påkrevd.")]
    [MaxLength(global::SportForAlle.Api.Models.Staff.NameMaxLength, ErrorMessage = "Navn er for langt.")]
    public required string Name { get; init; }

    [MaxLength(global::SportForAlle.Api.Models.Staff.JobTitleMaxLength, ErrorMessage = "Stilling er for lang.")]
    public string? JobTitle { get; init; }

    [EmailAddress(ErrorMessage = "E-post har ugyldig format.")]
    [MaxLength(global::SportForAlle.Api.Models.Staff.EmailMaxLength, ErrorMessage = "E-post er for lang.")]
    public string? Email { get; init; }

    [MaxLength(global::SportForAlle.Api.Models.Staff.PhoneMaxLength, ErrorMessage = "Telefon er for langt.")]
    public string? Phone { get; init; }
}
