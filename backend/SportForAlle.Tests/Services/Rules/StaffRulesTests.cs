using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Services.Rules;

public class StaffRulesTests
{
    private static readonly FakeClock _clock = new();

    [Fact]
    public void EnsureNotAlreadyLinked_allows_an_unlinked_staff_member()
    {
        Staff staff = new("Kari Nordmann", _clock);

        StaffRules.EnsureNotAlreadyLinked(staff);
    }

    [Fact]
    public void EnsureNotAlreadyLinked_rejects_a_staff_member_who_is_already_linked()
    {
        Staff staff = new("Kari Nordmann", _clock);
        staff.LinkAuth0User("auth0|abc123", _clock, staffId: null);

        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => StaffRules.EnsureNotAlreadyLinked(staff));

        Assert.Equal("StaffAlreadyLinked", exception.Reason);
    }

    [Fact]
    public void EnsureAuth0AccountNotLinkedElsewhere_allows_an_account_linked_to_no_one_else()
    {
        StaffRules.EnsureAuth0AccountNotLinkedElsewhere(isLinkedToAnotherStaff: false);
    }

    [Fact]
    public void EnsureAuth0AccountNotLinkedElsewhere_rejects_an_account_already_linked_to_someone_else()
    {
        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => StaffRules.EnsureAuth0AccountNotLinkedElsewhere(isLinkedToAnotherStaff: true));

        Assert.Equal("Auth0AccountAlreadyLinked", exception.Reason);
    }
}
