using SportForAlle.Api.Dtos.Bans;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class BanMapper
{
    public static BanResponse ToResponse(Ban ban) =>
        new(
            ban.Id,
            ban.BorrowerId,
            ban.Reason,
            ban.CreatedAt,
            ban.FeePaidAt,
            ban.LiftedAt,
            ban.IsActive);
}
