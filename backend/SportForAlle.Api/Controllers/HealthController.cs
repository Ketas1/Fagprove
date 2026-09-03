using Microsoft.AspNetCore.Mvc;
using SportForAlle.Api.Data;

namespace SportForAlle.Api.Controllers;

/// <summary>
/// Reports whether the API is running and whether it can reach the database.
/// Used to verify that all three layers are wired together.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController(AppDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAsync(CancellationToken cancellationToken)
    {
        bool databaseReachable = await dbContext.Database.CanConnectAsync(cancellationToken);

        return Ok(new HealthResponse(
            Status: "ok",
            Database: databaseReachable ? "up" : "down"));
    }

    public record HealthResponse(string Status, string Database);
}
