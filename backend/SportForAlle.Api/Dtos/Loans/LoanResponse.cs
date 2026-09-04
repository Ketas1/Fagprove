using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Loans;

public record LoanResponse(
    Guid Id,
    Guid BorrowerId,
    string BorrowerName,
    Guid EquipmentId,
    string EquipmentName,
    DateTimeOffset StartedAt,
    DateTimeOffset DueDate,
    DateTimeOffset? ReturnedAt,
    int? DaysLate,
    LoanStatus Status);
