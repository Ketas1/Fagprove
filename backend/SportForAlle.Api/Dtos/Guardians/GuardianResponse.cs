namespace SportForAlle.Api.Dtos.Guardians;

public record GuardianResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset? IdentityVerifiedAt,
    Guid? CreatedByStaffId,
    Guid? UpdatedByStaffId);
