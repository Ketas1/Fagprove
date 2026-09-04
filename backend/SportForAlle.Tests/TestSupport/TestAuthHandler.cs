using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Models;

namespace SportForAlle.Tests.TestSupport;

/// <summary>
/// A fake authentication scheme that always succeeds, so integration tests
/// can exercise an endpoint's authenticated path without a real Auth0 token.
/// See <see cref="AuthenticatedWebApplicationFactory{TEntryPoint}"/>.
/// </summary>
public class TestAuthHandler(
    IOptionsMonitor<TestAuthHandlerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    AppDbContext dbContext,
    IClock clock)
    : AuthenticationHandler<TestAuthHandlerOptions>(options, logger, encoder)
{
    public const string SchemeName = "Test";
    public const string DefaultSubject = "test-user";

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string subject = Options.Subject;

        if (Options.SeedLinkedStaff)
        {
            await EnsureLinkedStaffAsync(subject);
        }

        // Auth0 tokens present their "sub" claim, which ASP.NET Core remaps
        // to ClaimTypes.NameIdentifier by default - both are set here so
        // tests exercise the same claim-reading path
        // RequireLinkedStaffMiddleware uses against a real token.
        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, subject),
            new Claim("sub", subject),
        ];
        ClaimsIdentity identity = new(claims, SchemeName);
        AuthenticationTicket ticket = new(new ClaimsPrincipal(identity), SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    private async Task EnsureLinkedStaffAsync(string subject)
    {
        bool alreadyLinked = await dbContext.Staff.AnyAsync(staff => staff.Auth0UserId == subject);

        if (alreadyLinked)
        {
            return;
        }

        Staff staff = new("Test Ansatt", clock);
        staff.LinkAuth0User(subject, clock, staffId: staff.Id);
        dbContext.Staff.Add(staff);

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // A concurrent test request already created and linked this
            // subject's Staff row - the invariant this method exists to
            // guarantee (a linked Staff row exists for `subject`) still
            // holds either way. Detach the failed entity: this dbContext
            // instance is the same one the rest of this request's services
            // will use (both are resolved from the same request scope), and
            // a still-tracked "Added" entity would make their own,
            // unrelated SaveChangesAsync call fail on the same conflict.
            dbContext.Entry(staff).State = EntityState.Detached;
        }
    }
}
