using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Guardians;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class GuardianService(AppDbContext dbContext, IClock clock, CurrentUserContext currentUser)
{
    public async Task<IReadOnlyList<GuardianResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<Guardian> guardians = await dbContext.Guardians
            .OrderBy(guardian => guardian.Name)
            .ToListAsync(cancellationToken);

        return guardians.Select(GuardianMapper.ToResponse).ToList();
    }

    public async Task<GuardianResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        GuardianMapper.ToResponse(await FindAsync(id, cancellationToken));

    public async Task<GuardianResponse> CreateAsync(CreateGuardianRequest request, CancellationToken cancellationToken)
    {
        Guardian guardian = new(
            request.Name, request.Email, request.Phone, clock, currentUser.RequireStaffId(), request.IdentityVerified);

        dbContext.Guardians.Add(guardian);
        await dbContext.SaveChangesAsync(cancellationToken);

        return GuardianMapper.ToResponse(guardian);
    }

    public async Task<GuardianResponse> UpdateAsync(
        Guid id, UpdateGuardianRequest request, CancellationToken cancellationToken)
    {
        Guardian guardian = await FindAsync(id, cancellationToken);

        Guid staffId = currentUser.RequireStaffId();
        guardian.Rename(request.Name, clock, staffId);
        guardian.ChangeEmail(request.Email, clock, staffId);
        guardian.ChangePhone(request.Phone, clock, staffId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return GuardianMapper.ToResponse(guardian);
    }

    private async Task<Guardian> FindAsync(Guid id, CancellationToken cancellationToken) =>
        await dbContext.Guardians.FirstOrDefaultAsync(guardian => guardian.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke foresatt.");
}
