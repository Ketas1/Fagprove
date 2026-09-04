using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Middleware;

/// <summary>
/// Translates exceptions thrown by services into RFC 7807 <see cref="ProblemDetails"/>,
/// per docs/05-api.md. <see cref="DomainConflictException"/> and
/// <see cref="ForbiddenException"/> carry a machine-readable `reason`;
/// <see cref="NotFoundException"/> carries its own status; everything else
/// handled here is a defensive fallback - most commonly an
/// <see cref="ArgumentException"/> thrown by an entity's own validation -
/// and deliberately never echoes the exception's (English) message back to
/// the client, since <see cref="ProblemDetails"/> content is user-facing and
/// therefore Norwegian, see CLAUDE.md.
/// </summary>
public class ProblemDetailsExceptionHandler(ILogger<ProblemDetailsExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ProblemDetails? problemDetails = exception switch
        {
            NotFoundException notFound => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Fant ikke ressursen",
                Detail = notFound.Message,
            },
            ForbiddenException forbidden => WithReason(
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Ikke tilgang",
                    Detail = forbidden.Message,
                },
                forbidden.Reason),
            DomainConflictException conflict => WithReason(
                new ProblemDetails
                {
                    Status = conflict.StatusCode,
                    Title = "Regelbrudd",
                    Detail = conflict.Message,
                },
                conflict.Reason),
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Ugyldig forespørsel",
                Detail = "Ett eller flere felt i forespørselen er ugyldige.",
            },
            DbUpdateException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Regelbrudd",
                Detail = "Endringen kunne ikke lagres fordi den er i konflikt med eksisterende data.",
            },
            _ => null,
        };

        if (problemDetails is null)
        {
            return false;
        }

        // Domain/argument exceptions carry no personal data - only field and
        // rule names - so logging the exception itself is safe, see
        // CLAUDE.md, "Never log personal data".
        logger.LogInformation(exception, "Request rejected: {Title}", problemDetails.Title);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static ProblemDetails WithReason(ProblemDetails problemDetails, string reason)
    {
        problemDetails.Type = $"https://sportforalle.no/errors/{ToKebabCase(reason)}";
        problemDetails.Extensions["reason"] = reason;

        return problemDetails;
    }

    private static string ToKebabCase(string value)
    {
        StringBuilder builder = new();

        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];

            if (char.IsUpper(c) && i > 0)
            {
                builder.Append('-');
            }

            builder.Append(char.ToLowerInvariant(c));
        }

        return builder.ToString();
    }
}
