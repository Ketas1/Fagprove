using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Staff;
using SportForAlle.Api.Middleware;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/staff")]
public class StaffController(StaffService service) : ControllerBase
{
    /// <summary>
    /// Reachable while unlinked, so a not-yet-linked user can check whether
    /// their profile already exists before creating a duplicate.
    /// </summary>
    [AllowUnlinkedStaff]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<StaffResponse>>> GetAllAsync(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [AllowUnlinkedStaff]
    [HttpPost]
    public async Task<ActionResult<StaffResponse>> CreateAsync(
        CreateStaffRequest request, CancellationToken cancellationToken)
    {
        StaffResponse response = await service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [AllowUnlinkedStaff]
    [HttpPost("{id:guid}/link-me")]
    public async Task<ActionResult<StaffResponse>> LinkMeAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.LinkMeAsync(id, cancellationToken));

    /// <summary>
    /// Lets the frontend ask "which Staff profile, if any, is mine" without
    /// fetching the full list (which exposes every profile's
    /// <see cref="StaffResponse.Auth0UserId"/>). Reachable while unlinked -
    /// that is the normal case this exists for.
    /// </summary>
    [AllowUnlinkedStaff]
    [HttpGet("me")]
    public async Task<ActionResult<StaffResponse>> GetMeAsync(CancellationToken cancellationToken) =>
        Ok(await service.GetMeAsync(cancellationToken));
}
