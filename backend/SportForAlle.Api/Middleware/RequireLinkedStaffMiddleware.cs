using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Models;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Middleware;

/// <summary>
/// Requires that an authenticated request also come from a Staff member
/// linked to their Auth0 account - see
/// docs/adr/0019-staff-auth0-mapping.md. Authentication alone only proves
/// *who* the caller is, not that they have been provisioned as staff. Runs
/// after <c>UseAuthorization()</c>, so it only ever sees requests that
/// already passed the base authenticated-user fallback policy, and skips
/// entirely for endpoints marked <see cref="AllowUnlinkedStaffAttribute"/>
/// or <c>[AllowAnonymous]</c> - the Staff bootstrap endpoints, the health
/// check, and the dev-only Scalar/OpenAPI routes specifically.
/// </summary>
public class RequireLinkedStaffMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, CurrentUserContext currentUser)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context);
            return;
        }

        // ASP.NET Core remaps the "sub" claim to ClaimTypes.NameIdentifier
        // by default (MapInboundClaims, on unless explicitly disabled - not
        // disabled here), so a real Auth0 token surfaces it under the
        // mapped name, not the literal "sub". Checked both ways rather than
        // assuming which one applies.
        string? subject = context.User.FindFirst("sub")?.Value
            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        currentUser.Auth0Subject = subject;

        Endpoint? endpoint = context.GetEndpoint();
        bool exempt = endpoint?.Metadata.GetMetadata<AllowUnlinkedStaffAttribute>() is not null
            || endpoint?.Metadata.GetMetadata<IAllowAnonymous>() is not null;

        if (!exempt && subject is not null)
        {
            Staff staff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Auth0UserId == subject)
                ?? throw new ForbiddenException(
                    "StaffNotLinked", "Kontoen er ikke koblet til en ansattprofil ennå.");

            currentUser.StaffId = staff.Id;
        }

        await next(context);
    }
}
