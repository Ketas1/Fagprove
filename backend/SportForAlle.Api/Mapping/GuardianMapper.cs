using SportForAlle.Api.Dtos.Guardians;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class GuardianMapper
{
    public static GuardianResponse ToResponse(Guardian guardian) =>
        new(
            guardian.Id,
            guardian.Name,
            guardian.Email,
            guardian.Phone,
            guardian.IdentityVerifiedAt,
            guardian.ArchivedAt,
            guardian.AnonymisedAt,
            guardian.CreatedByStaffId,
            guardian.UpdatedByStaffId);
}
