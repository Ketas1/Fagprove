using SportForAlle.Api.Dtos.Notes;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class NoteMapper
{
    public static NoteResponse ToResponse(Note note) =>
        new(note.Id, note.BorrowerId, note.Text, note.CreatedAt, note.CreatedByStaffId);
}
