namespace SportForAlle.Api.Dtos.Notes;

public record NoteResponse(Guid Id, Guid BorrowerId, string Text, DateTimeOffset CreatedAt, Guid? CreatedByStaffId);
