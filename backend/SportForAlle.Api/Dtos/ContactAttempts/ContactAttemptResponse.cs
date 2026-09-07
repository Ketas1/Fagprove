using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.ContactAttempts;

public record ContactAttemptResponse(
    Guid Id,
    Guid LoanId,
    ContactMethod Method,
    string Outcome,
    DateTimeOffset CreatedAt,
    Guid? CreatedByStaffId);
