using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.EquipmentCategories;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/equipment-categories")]
public class EquipmentCategoriesController(EquipmentCategoryService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EquipmentCategoryResponse>>> GetAllAsync(
        CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpPost]
    public async Task<ActionResult<EquipmentCategoryResponse>> CreateAsync(
        CreateEquipmentCategoryRequest request, CancellationToken cancellationToken)
    {
        EquipmentCategoryResponse response = await service.CreateAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EquipmentCategoryResponse>> RenameAsync(
        Guid id, RenameEquipmentCategoryRequest request, CancellationToken cancellationToken) =>
        Ok(await service.RenameAsync(id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
