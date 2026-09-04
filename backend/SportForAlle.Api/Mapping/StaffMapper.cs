using SportForAlle.Api.Dtos.Staff;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class StaffMapper
{
    public static StaffResponse ToResponse(Staff staff) =>
        new(staff.Id, staff.Name, staff.Auth0UserId);
}
