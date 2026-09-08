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
        Staff staff = new(request.Name, clock, request.JobTitle, request.Email, request.Phone);

        dbContext.Staff.Add(staff);
        await dbContext.SaveChangesAsync(cancellationToken);

        return StaffMapper.ToResponse(staff);
    }

    /// <summary>
    /// Edits a colleague's profile. Auth0UserId is not touched here - linking
    /// stays owner-driven through LinkMeAsync, so editing a profile can never
    /// hand someone else's account to a different person.
    /// </summary>
    /// <summary>
    /// Removes an employee profile. The audit columns on every entity point at
    /// Staff with ON DELETE SET NULL, so historical rows survive with an
    /// unknown author rather than disappearing. Deleting the profile the
    /// caller is signed in as is refused - see StaffRules.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Staff staff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke ansatt.");

        StaffRules.EnsureNotDeletingOwnProfile(staff, currentUser.Auth0Subject);

        dbContext.Staff.Remove(staff);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<StaffResponse> UpdateAsync(
        Guid id, UpdateStaffRequest request, CancellationToken cancellationToken)
    {
        Staff staff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke ansatt.");

        staff.Update(request.Name, request.JobTitle, request.Email, request.Phone, clock, currentUser.RequireStaffId());
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

    /// <summary>
    /// The caller's own Staff profile, found by their Auth0 subject rather
    /// than an id in the route - there is nothing to link to yet if this
    /// returns 404, which is the normal state for a user who has not
    /// completed the bootstrap flow.
    /// </summary>
    public async Task<StaffResponse> GetMeAsync(CancellationToken cancellationToken)
    {
        string subject = currentUser.Auth0Subject
            ?? throw new InvalidOperationException(
                $"{nameof(CurrentUserContext)}.{nameof(CurrentUserContext.Auth0Subject)} was not populated " +
                "for an authenticated request.");

        Staff staff = await dbContext.Staff.FirstOrDefaultAsync(s => s.Auth0UserId == subject, cancellationToken)
            ?? throw new NotFoundException("Kontoen er ikke koblet til en ansattprofil ennå.");

        return StaffMapper.ToResponse(staff);
    }
}
