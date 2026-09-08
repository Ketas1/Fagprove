using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Guardians;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
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

    /// <summary>
    /// Hard delete, only once no borrower points at this guardian. Business
    /// rule 1 means a child cannot exist without one - see ADR-0026.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Guardian guardian = await FindAsync(id, cancellationToken);

        bool hasBorrowers = await dbContext.Borrowers
            .AnyAsync(borrower => borrower.GuardianId == id, cancellationToken);
        BorrowerRules.EnsureGuardianCanBeDeleted(hasBorrowers);

        dbContext.Guardians.Remove(guardian);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<GuardianResponse> ArchiveAsync(Guid id, CancellationToken cancellationToken)
    {
        Guardian guardian = await FindAsync(id, cancellationToken);
        guardian.Archive(clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return GuardianMapper.ToResponse(guardian);
    }

    public async Task<GuardianResponse> RestoreAsync(Guid id, CancellationToken cancellationToken)
    {
        Guardian guardian = await FindAsync(id, cancellationToken);
        guardian.Restore(clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return GuardianMapper.ToResponse(guardian);
    }

    /// <summary>
    /// Strips the contact details, and with them the contact attempts, whose
    /// free-text outcome can describe the conversation. See
    /// docs/09-lover-og-regler.md.
    /// </summary>
    public async Task<GuardianResponse> AnonymiseAsync(Guid id, CancellationToken cancellationToken)
    {
        Guardian guardian = await FindAsync(id, cancellationToken);

        List<Guid> borrowerIds = await dbContext.Borrowers
            .Where(borrower => borrower.GuardianId == id)
            .Select(borrower => borrower.Id)
            .ToListAsync(cancellationToken);

        List<ContactAttempt> attempts = await dbContext.ContactAttempts
            .Where(attempt => dbContext.Loans
                .Any(loan => loan.Id == attempt.LoanId && borrowerIds.Contains(loan.BorrowerId)))
            .ToListAsync(cancellationToken);
        dbContext.ContactAttempts.RemoveRange(attempts);

        guardian.Anonymise(clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return GuardianMapper.ToResponse(guardian);
    }

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
