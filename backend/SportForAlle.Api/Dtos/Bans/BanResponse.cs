namespace SportForAlle.Api.Dtos.Bans;

public record BanResponse(
    Guid Id,
    Guid BorrowerId,
    string Reason,
    DateTimeOffset CreatedAt,
    DateTimeOffset? FeePaidAt,
    DateTimeOffset? LiftedAt,
    bool IsActive);
