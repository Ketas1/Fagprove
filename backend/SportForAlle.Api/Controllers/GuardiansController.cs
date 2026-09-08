using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Guardians;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/guardians")]
public class GuardiansController(GuardianService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<GuardianResponse>>> GetAllAsync(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<GuardianResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<GuardianResponse>> CreateAsync(
        CreateGuardianRequest request, CancellationToken cancellationToken)
    {
        GuardianResponse response = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<GuardianResponse>> UpdateAsync(
        Guid id, UpdateGuardianRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    /// <summary>
    /// Hard delete. Refused with <c>409 GuardianHasBorrowers</c> while any
    /// child still points at this guardian - business rule 1 says a child
    /// cannot exist without one. See ADR-0026.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<GuardianResponse>> ArchiveAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.ArchiveAsync(id, cancellationToken));

    [HttpDelete("{id:guid}/archive")]
    public async Task<ActionResult<GuardianResponse>> RestoreAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.RestoreAsync(id, cancellationToken));

    /// <summary>Irreversible. Strips name, email and phone, and deletes the contact attempts.</summary>
    [HttpPost("{id:guid}/anonymise")]
    public async Task<ActionResult<GuardianResponse>> AnonymiseAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.AnonymiseAsync(id, cancellationToken));
}
