using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class NoteTests
{
    private static readonly FakeClock _clock = new();

    [Fact]
    public void Constructor_trims_the_text()
    {
        Note note = new(Guid.NewGuid(), "  Called about overdue skis.  ", _clock, Guid.NewGuid());

        Assert.Equal("Called about overdue skis.", note.Text);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_blank_text(string text)
    {
        Assert.Throws<ArgumentException>(() => new Note(Guid.NewGuid(), text, _clock, null));
    }

    [Fact]
    public void Rewrite_replaces_the_text_and_records_who_changed_it()
    {
        Note note = new(Guid.NewGuid(), "Original text.", _clock, Guid.NewGuid());
        Guid staffId = Guid.NewGuid();

        note.Rewrite("Corrected text.", _clock, staffId);

        Assert.Equal("Corrected text.", note.Text);
        Assert.Equal(staffId, note.UpdatedByStaffId);
    }

    [Fact]
    public void Rewrite_rejects_blank_text_and_leaves_the_original_intact()
    {
        Note note = new(Guid.NewGuid(), "Original text.", _clock, Guid.NewGuid());

        Assert.Throws<ArgumentException>(() => note.Rewrite("   ", _clock, Guid.NewGuid()));
        Assert.Equal("Original text.", note.Text);
    }
}
