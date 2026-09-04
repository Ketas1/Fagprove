using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Equipment;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/equipment")]
public class EquipmentController(EquipmentService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentResponse>>> GetAllAsync(
        [FromQuery] EquipmentStatus? status, [FromQuery] Guid? categoryId, CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(status, categoryId, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EquipmentResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EquipmentResponse>> CreateAsync(
        CreateEquipmentRequest request, CancellationToken cancellationToken)
    {
        EquipmentResponse response = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipmentResponse>> UpdateAsync(
        Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
