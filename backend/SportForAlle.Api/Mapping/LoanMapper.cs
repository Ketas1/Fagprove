using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class LoanMapper
{
    /// <summary>
    /// <paramref name="borrowerName"/>, <paramref name="equipmentName"/> and the
    /// two contact figures are passed in rather than read from navigation
    /// properties - <see cref="Loan"/> has none, see docs/04-databasedesign.md.
    /// Callers join them in themselves.
    /// </summary>
    public static LoanResponse ToResponse(
        Loan loan,
        string borrowerName,
        string equipmentName,
        int contactAttemptCount,
        DateTimeOffset? lastContactedAt) =>
        new(
            loan.Id,
            loan.BorrowerId,
            borrowerName,
            loan.EquipmentId,
            equipmentName,
            loan.StartedAt,
            loan.DueDate,
            loan.ReturnedAt,
            loan.DaysLate,
            loan.Status,
            contactAttemptCount,
            lastContactedAt,
            loan.CreatedByStaffId,
            loan.UpdatedByStaffId);
}
