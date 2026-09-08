using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Bans;
using SportForAlle.Api.Dtos.Borrowers;
using SportForAlle.Api.Dtos.Notes;
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

    /// <summary>
    /// Hard delete. Refused with <c>409 BorrowerHasHistory</c> if any loan,
    /// ban or note references the borrower - those are archived and later
    /// anonymised instead, see ADR-0026.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<BorrowerResponse>> ArchiveAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.ArchiveAsync(id, cancellationToken));

    [HttpDelete("{id:guid}/archive")]
    public async Task<ActionResult<BorrowerResponse>> RestoreAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.RestoreAsync(id, cancellationToken));

    /// <summary>Irreversible. Answers a GDPR article 17 request for a borrower who has loan history.</summary>
    [HttpPost("{id:guid}/anonymise")]
    public async Task<ActionResult<BorrowerResponse>> AnonymiseAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.AnonymiseAsync(id, cancellationToken));

    [HttpGet("{id:guid}/ban")]
    public async Task<ActionResult<BanResponse>> GetCurrentBanAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetCurrentBanAsync(id, cancellationToken));

    [HttpPost("{id:guid}/ban")]
    public async Task<ActionResult<BanResponse>> BanAsync(
        Guid id, BanBorrowerRequest request, CancellationToken cancellationToken) =>
        Ok(await service.BanAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/ban/fee-paid")]
    public async Task<ActionResult<BanResponse>> RecordBanFeePaidAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.RecordBanFeePaidAsync(id, cancellationToken));

    [HttpDelete("{id:guid}/ban")]
    public async Task<IActionResult> LiftBanAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.LiftBanAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<ActionResult<NoteResponse>> AddNoteAsync(
        Guid id, CreateNoteRequest request, CancellationToken cancellationToken) =>
        Ok(await service.AddNoteAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/notes")]
    public async Task<ActionResult<IReadOnlyList<NoteResponse>>> GetNotesAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetNotesAsync(id, cancellationToken));
}
