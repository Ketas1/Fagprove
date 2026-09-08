using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Reports;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

/// <summary>
/// The two reports in docs/03-domenemodell.md, plus the timeline breakdown the
/// trend chart needs. Aggregated counts only - see docs/09-lover-og-regler.md.
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService service) : ControllerBase
{
    /// <summary>Report 1 of 2. Omit <c>from</c>/<c>to</c> for the whole history.</summary>
    [HttpGet("loans")]
    public async Task<ActionResult<LoanSummaryReportResponse>> GetLoanSummaryAsync(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await service.GetLoanSummaryAsync(from, to, cancellationToken));

    /// <summary>Report 2 of 2. The same figures, split by the borrower's age group.</summary>
    [HttpGet("age-groups")]
    public async Task<ActionResult<AgeGroupReportResponse>> GetAgeGroupsAsync(
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to, CancellationToken cancellationToken) =>
        Ok(await service.GetAgeGroupsAsync(from, to, cancellationToken));

    /// <summary>Loan counts per bucket for the trend chart. <c>interval</c> defaults to month.</summary>
    [HttpGet("timeline")]
    public async Task<ActionResult<LoanTimelineReportResponse>> GetTimelineAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] TimelineInterval? interval,
        CancellationToken cancellationToken) =>
        Ok(await service.GetTimelineAsync(from, to, interval ?? TimelineInterval.Month, cancellationToken));

    /// <summary>Not surfaced by any page - see docs/03-domenemodell.md.</summary>
    [HttpGet("popular-equipment")]
    public async Task<ActionResult<PopularEquipmentReportResponse>> GetPopularEquipmentAsync(
        CancellationToken cancellationToken) =>
        Ok(await service.GetPopularEquipmentAsync(cancellationToken));
}
