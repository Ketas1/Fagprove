using SportForAlle.Api.Helpers;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services.Rules;

/// <summary>
/// Business rules that only need a <see cref="DateOnly"/> and a clock, with
/// no database access - kept separate from <c>BorrowerService</c> so they
/// can be unit tested directly, per docs/07-testing.md.
/// </summary>
public static class BorrowerRules
{
    public const int MinimumAge = 3;
    public const int MaximumAge = 18;

    /// <summary>
    /// Business rule 4 in docs/03-domenemodell.md: a borrower must be
    /// between 3 and 18 years old. Checked once, at registration - not
    /// re-checked at loan time.
    /// </summary>
    public static void EnsureAgeInRange(DateOnly dateOfBirth, IClock clock)
    {
        int age = CalculateAge(dateOfBirth, clock.UtcNow);

        if (age < MinimumAge || age > MaximumAge)
        {
            throw new DomainConflictException(
                "BorrowerOutsideAgeRange",
                $"Låntakeren må være mellom {MinimumAge} og {MaximumAge} år.",
                StatusCodes.Status422UnprocessableEntity);
        }
    }

    /// <summary>
    /// Shared with <see cref="ReportService"/>, which buckets loans by the
    /// borrower's age at <c>Loan.StartedAt</c> - the same calculation,
    /// evaluated as of a different instant, kept in one place per ADR-0011's
    /// "not written twice" rule.
    /// </summary>
    internal static int CalculateAge(DateOnly dateOfBirth, DateTimeOffset asOf)
    {
        DateOnly today = DateOnly.FromDateTime(asOf.UtcDateTime);
        int age = today.Year - dateOfBirth.Year;

        if (dateOfBirth > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }
}
