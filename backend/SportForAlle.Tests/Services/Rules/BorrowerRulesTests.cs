using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Services.Rules;

public class BorrowerRulesTests
{
    private static readonly FakeClock _clock = new(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));

    [Theory]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(18)]
    public void EnsureAgeInRange_accepts_ages_inside_the_allowed_range(int age)
    {
        DateOnly dateOfBirth = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime).AddYears(-age);

        BorrowerRules.EnsureAgeInRange(dateOfBirth, _clock);
    }

    [Fact]
    public void EnsureAgeInRange_rejects_a_borrower_who_has_not_turned_three_yet()
    {
        DateOnly dateOfBirth = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime).AddYears(-3).AddDays(1);

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => BorrowerRules.EnsureAgeInRange(dateOfBirth, _clock));

        Assert.Equal("BorrowerOutsideAgeRange", exception.Reason);
        Assert.Equal(422, exception.StatusCode);
    }

    [Fact]
    public void EnsureAgeInRange_rejects_a_borrower_who_has_already_turned_nineteen()
    {
        DateOnly dateOfBirth = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime).AddYears(-19);

        Assert.Throws<DomainConflictException>(() => BorrowerRules.EnsureAgeInRange(dateOfBirth, _clock));
    }

    [Fact]
    public void EnsureAgeInRange_accepts_a_borrower_who_turns_eighteen_today()
    {
        DateOnly dateOfBirth = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime).AddYears(-18);

        BorrowerRules.EnsureAgeInRange(dateOfBirth, _clock);
    }
}
