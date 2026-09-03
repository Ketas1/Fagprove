using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class ContactAttemptTests
{
    private static readonly FakeClock _clock = new();

    [Fact]
    public void Constructor_trims_the_outcome()
    {
        ContactAttempt attempt = new(Guid.NewGuid(), ContactMethod.Phone, "  No answer.  ", _clock, Guid.NewGuid());

        Assert.Equal("No answer.", attempt.Outcome);
        Assert.Equal(ContactMethod.Phone, attempt.Method);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_outcome(string outcome)
    {
        Assert.Throws<ArgumentException>(
            () => new ContactAttempt(Guid.NewGuid(), ContactMethod.Email, outcome, _clock, null));
    }

    [Fact]
    public void Constructor_uses_the_creation_time_as_the_attempt_time()
    {
        Guid staffId = Guid.NewGuid();
        ContactAttempt attempt = new(Guid.NewGuid(), ContactMethod.Email, "Reached the guardian.", _clock, staffId);

        Assert.Equal(_clock.UtcNow, attempt.CreatedAt);
        Assert.Equal(staffId, attempt.CreatedByStaffId);
    }
}
