using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/loans")]
public class LoansController(LoanService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LoanResponse>>> GetAllAsync(
        [FromQuery] LoanStatus? status, CancellationToken cancellationToken) =>
        Ok(await service.GetAllAsync(status, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<LoanResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<LoanResponse>> RegisterAsync(
        CreateLoanRequest request, CancellationToken cancellationToken)
    {
        LoanResponse response = await service.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = response.Id }, response);
    }

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<LoanResponse>> ReturnAsync(
        Guid id, ReturnLoanRequest request, CancellationToken cancellationToken) =>
        Ok(await service.ReturnAsync(id, request, cancellationToken));
}
