namespace SportForAlle.Api.Dtos.Guardians;

public record GuardianResponse(
    Guid Id,
    string Name,
    string Email,
    string Phone,
    DateTimeOffset? IdentityVerifiedAt,
    DateTimeOffset? ArchivedAt,
    DateTimeOffset? AnonymisedAt,
    Guid? CreatedByStaffId,
    Guid? UpdatedByStaffId);
