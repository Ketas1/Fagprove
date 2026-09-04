using SportForAlle.Api.Dtos.Borrowers;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class BorrowerMapper
{
    /// <summary>
    /// <paramref name="guardianName"/> is passed in rather than read from a
    /// navigation property - <see cref="Borrower"/> has none, see
    /// docs/04-databasedesign.md. Callers join it in themselves.
    /// </summary>
    public static BorrowerResponse ToResponse(Borrower borrower, string guardianName) =>
        new(
            borrower.Id,
            borrower.Name,
            borrower.DateOfBirth,
            borrower.GuardianId,
            guardianName,
            borrower.LateReturnCount,
            borrower.IsUnreliable,
            borrower.Status);
}
