using System.ComponentModel.DataAnnotations;

namespace SportForAlle.Api.Dtos.Staff;

/// <summary>
/// Name is the only required field; the rest are optional and a blank value
/// clears what was recorded. Auth0UserId is deliberately absent - the link is
/// established by the owner through <c>POST /api/staff/{id}/link-me</c>, never
/// by editing a profile.
/// </summary>
public record UpdateStaffRequest
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
