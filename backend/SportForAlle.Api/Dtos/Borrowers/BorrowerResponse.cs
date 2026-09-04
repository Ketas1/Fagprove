using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Borrowers;

public record BorrowerResponse(
    Guid Id,
    string Name,
    DateOnly DateOfBirth,
    Guid GuardianId,
    string GuardianName,
    int LateReturnCount,
    bool IsUnreliable,
    BorrowerStatus Status);
