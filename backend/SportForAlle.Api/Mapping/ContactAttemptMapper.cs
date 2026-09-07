using SportForAlle.Api.Dtos.ContactAttempts;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class ContactAttemptMapper
{
    public static ContactAttemptResponse ToResponse(ContactAttempt attempt) =>
        new(attempt.Id, attempt.LoanId, attempt.Method, attempt.Outcome, attempt.CreatedAt, attempt.CreatedByStaffId);
}
