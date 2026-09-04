using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Data;
using SportForAlle.Api.Middleware;

namespace SportForAlle.Api.Controllers;

/// <summary>
/// Reports whether the API is running and whether it can reach the database.
/// Used to verify that all three layers are wired together.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext dbContext, ILogger<HealthController> logger) : ControllerBase
{
    /// <summary>
    /// An operational check, not domain data - reachable by any
    /// authenticated user regardless of whether they are linked to a Staff
    /// profile yet.
    /// </summary>
    [AllowUnlinkedStaff]
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        bool databaseReachable = await dbContext.Database.CanConnectAsync(cancellationToken);

        logger.LogInformation("Health check requested. Database reachable: {DatabaseReachable}", databaseReachable);

        return Ok(new HealthResponse(
            Status: "ok",
            Database: databaseReachable ? "up" : "down"));
    }

    public record HealthResponse(string Status, string Database);
}
