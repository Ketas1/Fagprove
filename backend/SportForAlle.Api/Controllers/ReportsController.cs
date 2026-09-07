using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Dtos.Reports;
using SportForAlle.Api.Services;

namespace SportForAlle.Api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController(ReportService service) : ControllerBase
{
    [HttpGet("loans")]
    public async Task<ActionResult<LoanCountReportResponse>> GetLoanCountAsync(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken) =>
        Ok(await service.GetLoanCountAsync(from, to, cancellationToken));

    [HttpGet("age-groups")]
    public async Task<ActionResult<AgeGroupReportResponse>> GetAgeGroupsAsync(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken cancellationToken) =>
        Ok(await service.GetAgeGroupsAsync(from, to, cancellationToken));

    [HttpGet("popular-equipment")]
    public async Task<ActionResult<PopularEquipmentReportResponse>> GetPopularEquipmentAsync(
        CancellationToken cancellationToken) =>
        Ok(await service.GetPopularEquipmentAsync(cancellationToken));

    [HttpGet("overdue-summary")]
    public async Task<ActionResult<OverdueSummaryReportResponse>> GetOverdueSummaryAsync(
        CancellationToken cancellationToken) =>
        Ok(await service.GetOverdueSummaryAsync(cancellationToken));
}
