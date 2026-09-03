using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class BorrowerTests
{
    private static readonly FakeClock _clock = new();
    private static readonly Guid _guardianId = Guid.NewGuid();

    private static Borrower CreateBorrower() =>
        new("Ola Nordmann", new DateOnly(2018, 6, 1), _guardianId, _clock, createdByStaffId: null);

    [Fact]
    public void Constructor_starts_active_and_reliable()
    {
        Borrower borrower = CreateBorrower();

        Assert.Equal(BorrowerStatus.Active, borrower.Status);
        Assert.False(borrower.IsUnreliable);
        Assert.Equal(0, borrower.LateReturnCount);
    }

    [Fact]
    public void Constructor_rejects_a_date_of_birth_in_the_future()
    {
        DateOnly tomorrow = DateOnly.FromDateTime(_clock.UtcNow.UtcDateTime).AddDays(1);

        Assert.Throws<ArgumentException>(() => new Borrower("Ola", tomorrow, _guardianId, _clock, null));
    }

    [Fact]
    public void RecordLateReturn_increments_the_counter_and_flags_the_borrower()
    {
        Borrower borrower = CreateBorrower();
        Guid staffId = Guid.NewGuid();

        borrower.RecordLateReturn(_clock, staffId);
        borrower.RecordLateReturn(_clock, staffId);

        Assert.Equal(2, borrower.LateReturnCount);
        Assert.True(borrower.IsUnreliable);
    }

    [Fact]
    public void IsUnreliable_is_independent_from_Banned_status()
    {
        Borrower borrower = CreateBorrower();

        borrower.RecordLateReturn(_clock, staffId: null);

        Assert.True(borrower.IsUnreliable);
        Assert.Equal(BorrowerStatus.Active, borrower.Status);
    }

    [Fact]
    public void Ban_sets_status_to_banned_and_records_a_ban_with_the_reason()
    {
        Borrower borrower = CreateBorrower();

        borrower.Ban("Escalated after no response to three contact attempts.", _clock, Guid.NewGuid());

        Assert.Equal(BorrowerStatus.Banned, borrower.Status);
        Ban ban = Assert.Single(borrower.Bans);
        Assert.True(ban.IsActive);
        Assert.Equal("Escalated after no response to three contact attempts.", ban.Reason);
    }

    [Fact]
    public void Ban_rejects_banning_a_borrower_who_is_already_banned()
    {
        Borrower borrower = CreateBorrower();
        borrower.Ban("First reason.", _clock, staffId: null);

        Assert.Throws<InvalidOperationException>(() => borrower.Ban("Second reason.", _clock, staffId: null));
    }

    [Fact]
    public void LiftBan_requires_the_fee_to_be_recorded_as_paid_first()
    {
        Borrower borrower = CreateBorrower();
        borrower.Ban("Reason.", _clock, staffId: null);

        Assert.Throws<InvalidOperationException>(() => borrower.LiftBan(_clock, staffId: null));
    }

    [Fact]
    public void LiftBan_restores_active_status_once_the_fee_is_paid()
    {
        Borrower borrower = CreateBorrower();
        Guid staffId = Guid.NewGuid();
        borrower.Ban("Reason.", _clock, staffId);

        borrower.RecordBanFeePaid(_clock, staffId);
        borrower.LiftBan(_clock, staffId);

        Assert.Equal(BorrowerStatus.Active, borrower.Status);
        Ban ban = Assert.Single(borrower.Bans);
        Assert.False(ban.IsActive);
        Assert.Equal(staffId, ban.LiftedByStaffId);
    }

    [Fact]
    public void LiftBan_rejects_lifting_a_ban_that_does_not_exist()
    {
        Borrower borrower = CreateBorrower();

        Assert.Throws<InvalidOperationException>(() => borrower.LiftBan(_clock, staffId: null));
    }

    [Fact]
    public void A_borrower_can_be_banned_again_after_a_ban_is_lifted()
    {
        Borrower borrower = CreateBorrower();
        borrower.Ban("First reason.", _clock, staffId: null);
        borrower.RecordBanFeePaid(_clock, staffId: null);
        borrower.LiftBan(_clock, staffId: null);

        borrower.Ban("Second reason.", _clock, staffId: null);

        Assert.Equal(BorrowerStatus.Banned, borrower.Status);
        Assert.Equal(2, borrower.Bans.Count);
    }
}
