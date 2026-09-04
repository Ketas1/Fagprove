using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class LoanMapper
{
    /// <summary>
    /// <paramref name="borrowerName"/> and <paramref name="equipmentName"/>
    /// are passed in rather than read from navigation properties -
    /// <see cref="Loan"/> has none, see docs/04-databasedesign.md. Callers
    /// join them in themselves.
    /// </summary>
    public static LoanResponse ToResponse(Loan loan, string borrowerName, string equipmentName) =>
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
            loan.CreatedByStaffId,
            loan.UpdatedByStaffId);
}
