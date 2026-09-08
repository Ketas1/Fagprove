namespace SportForAlle.Api.Dtos.Staff;

/// <summary>
/// <paramref name="JobTitle"/> is the shop position ("Butikkleder"), not an
/// authorisation role - see docs/adr/0019-staff-auth0-mapping.md. All three
/// added fields are optional and null when nothing has been recorded.
/// </summary>
public record StaffResponse(
    Guid Id,
    string Name,
    string? JobTitle,
    string? Email,
    string? Phone,
    string? Auth0UserId);
