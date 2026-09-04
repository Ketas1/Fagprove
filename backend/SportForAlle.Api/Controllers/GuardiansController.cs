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
}
