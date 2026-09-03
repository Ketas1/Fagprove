using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class GuardianTests
{
    private static readonly FakeClock _clock = new();

    private static Guardian CreateGuardian() =>
        new("Kari Nordmann", "kari@example.no", "12345678", _clock, createdByStaffId: null);

    [Fact]
    public void Constructor_trims_all_fields()
    {
        Guardian guardian = new("  Kari  ", " kari@example.no ", " 12345678 ", _clock, createdByStaffId: null);

        Assert.Equal("Kari", guardian.Name);
        Assert.Equal("kari@example.no", guardian.Email);
        Assert.Equal("12345678", guardian.Phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_name(string name)
    {
        Assert.Throws<ArgumentException>(() => new Guardian(name, "kari@example.no", "12345678", _clock, null));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    [InlineData("@missing-local.no")]
    public void Constructor_rejects_an_invalid_email(string email)
    {
        Assert.Throws<ArgumentException>(() => new Guardian("Kari", email, "12345678", _clock, null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_phone(string phone)
    {
        Assert.Throws<ArgumentException>(() => new Guardian("Kari", "kari@example.no", phone, _clock, null));
    }

    [Fact]
    public void ChangeEmail_replaces_the_email_and_records_who_changed_it()
    {
        Guardian guardian = CreateGuardian();
        Guid staffId = Guid.NewGuid();

        guardian.ChangeEmail("nykari@example.no", _clock, staffId);

        Assert.Equal("nykari@example.no", guardian.Email);
        Assert.Equal(staffId, guardian.UpdatedByStaffId);
    }

    [Fact]
    public void ChangePhone_rejects_a_phone_that_is_too_long()
    {
        Guardian guardian = CreateGuardian();
        string tooLong = new('1', Guardian.PhoneMaxLength + 1);

        Assert.Throws<ArgumentException>(() => guardian.ChangePhone(tooLong, _clock, staffId: null));
    }

    [Fact]
    public void Borrowers_starts_empty()
    {
        Guardian guardian = CreateGuardian();

        Assert.Empty(guardian.Borrowers);
    }
}
