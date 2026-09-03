using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class StaffTests
{
    private static readonly FakeClock _clock = new();

    [Fact]
    public void Constructor_trims_the_name()
    {
        Staff staff = new("  Kari Nordmann  ", _clock);

        Assert.Equal("Kari Nordmann", staff.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_name(string name)
    {
        Assert.Throws<ArgumentException>(() => new Staff(name, _clock));
    }

    [Fact]
    public void Constructor_has_no_auth0_user_yet()
    {
        Staff staff = new("Kari Nordmann", _clock);

        Assert.Null(staff.Auth0UserId);
    }

    [Fact]
    public void LinkAuth0User_stores_the_trimmed_subject_id()
    {
        Staff staff = new("Kari Nordmann", _clock);

        staff.LinkAuth0User("  auth0|abc123  ", _clock, staffId: null);

        Assert.Equal("auth0|abc123", staff.Auth0UserId);
    }

    [Fact]
    public void LinkAuth0User_rejects_a_blank_id()
    {
        Staff staff = new("Kari Nordmann", _clock);

        Assert.Throws<ArgumentException>(() => staff.LinkAuth0User("   ", _clock, staffId: null));
    }
}
