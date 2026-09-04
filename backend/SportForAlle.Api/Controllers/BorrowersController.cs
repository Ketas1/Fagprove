using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Borrowers;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/borrowers")]
public class BorrowersController(BorrowerService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BorrowerResponse>>> GetAllAsync(CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BorrowerResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BorrowerResponse>> CreateAsync(
        CreateBorrowerRequest request, CancellationToken cancellationToken)
    {
        BorrowerResponse response = await service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BorrowerResponse>> UpdateAsync(
        Guid id, UpdateBorrowerRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));
}
