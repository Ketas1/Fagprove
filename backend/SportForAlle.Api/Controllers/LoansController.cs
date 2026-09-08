using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.ContactAttempts;
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

    /// <summary>
    /// Corrects an open loan. <c>409 LoanAlreadyClosed</c> for a Returned or
    /// Lost loan, <c>409 BorrowerBanned</c>/<c>BorrowerHasOverdueLoan</c> if
    /// the new borrower is not eligible, <c>409 EquipmentNotAvailable</c> if
    /// the new equipment is already out.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<LoanResponse>> UpdateAsync(
        Guid id, UpdateLoanRequest request, CancellationToken cancellationToken) =>
        Ok(await service.UpdateAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/return")]
    public async Task<ActionResult<LoanResponse>> ReturnAsync(
        Guid id, ReturnLoanRequest request, CancellationToken cancellationToken) =>
        Ok(await service.ReturnAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/mark-lost")]
    public async Task<ActionResult<LoanResponse>> MarkLostAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await service.MarkLostAsync(id, cancellationToken));

    [HttpPost("{id:guid}/contact-attempts")]
    public async Task<ActionResult<ContactAttemptResponse>> LogContactAttemptAsync(
        Guid id, LogContactAttemptRequest request, CancellationToken cancellationToken) =>
        Ok(await service.LogContactAttemptAsync(id, request, cancellationToken));

    [HttpGet("{id:guid}/contact-attempts")]
    public async Task<ActionResult<IReadOnlyList<ContactAttemptResponse>>> GetContactAttemptsAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await service.GetContactAttemptsAsync(id, cancellationToken));

    [HttpPost("{id:guid}/send-followup-email")]
    public async Task<ActionResult<ContactAttemptResponse>> SendFollowUpEmailAsync(
        Guid id, CancellationToken cancellationToken) =>
        Ok(await service.SendFollowUpEmailAsync(id, cancellationToken));
}
