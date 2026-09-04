using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Staff;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class StaffService(AppDbContext dbContext, IClock clock, CurrentUserContext currentUser)
{
    public async Task<IReadOnlyList<StaffResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<Staff> staffMembers = await dbContext.Staff
            .OrderBy(staff => staff.Name)
            .ToListAsync(cancellationToken);

        return staffMembers.Select(StaffMapper.ToResponse).ToList();
    }

    public async Task<StaffResponse> CreateAsync(CreateStaffRequest request, CancellationToken cancellationToken)
    {
        Staff staff = new(request.Name, clock);

        dbContext.Staff.Add(staff);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StaffMapper.ToResponse(staff);
    }

    /// <summary>
    /// Links the caller's own Auth0 account (read from
    /// <see cref="CurrentUserContext.Auth0Subject"/>, populated by
    /// <see cref="Middleware.RequireLinkedStaffMiddleware"/> for any
    /// authenticated request) to the given Staff profile.
    /// </summary>
    public async Task<StaffResponse> LinkMeAsync(Guid id, CancellationToken cancellationToken)
    {
        string subject = currentUser.Auth0Subject
            ?? throw new InvalidOperationException(
                $"{nameof(CurrentUserContext)}.{nameof(CurrentUserContext.Auth0Subject)} was not populated " +
                "for an authenticated request.");

        Staff staff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke ansatt.");

        StaffRules.EnsureNotAlreadyLinked(staff);

        bool linkedElsewhere = await dbContext.Staff
            .AnyAsync(s => s.Auth0UserId == subject && s.Id != id, cancellationToken);
        StaffRules.EnsureAuth0AccountNotLinkedElsewhere(linkedElsewhere);

        staff.LinkAuth0User(subject, clock, staffId: staff.Id);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StaffMapper.ToResponse(staff);
    }
}
