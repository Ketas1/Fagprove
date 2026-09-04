using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Borrowers;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class BorrowerService(AppDbContext dbContext, IClock clock)
{
    public async Task<IReadOnlyList<BorrowerResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<BorrowerWithGuardianName> rows = await QueryWithGuardianName().ToListAsync(cancellationToken);

        return rows.Select(row => BorrowerMapper.ToResponse(row.Borrower, row.GuardianName)).ToList();
    }

    public async Task<BorrowerResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        BorrowerWithGuardianName row = await QueryWithGuardianName(id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        return BorrowerMapper.ToResponse(row.Borrower, row.GuardianName);
    }

    public async Task<BorrowerResponse> CreateAsync(CreateBorrowerRequest request, CancellationToken cancellationToken)
    {
        bool linksExistingGuardian = request.GuardianId.HasValue;
        bool createsNewGuardian = request.NewGuardian is not null;

        if (linksExistingGuardian == createsNewGuardian)
        {
            throw new ArgumentException(
                "Exactly one of GuardianId or NewGuardian must be provided.", nameof(request));
        }

        BorrowerRules.EnsureAgeInRange(request.DateOfBirth, clock);

        Guardian guardian = linksExistingGuardian
            ? await dbContext.Guardians.FirstOrDefaultAsync(g => g.Id == request.GuardianId, cancellationToken)
                ?? throw new NotFoundException("Fant ikke foresatt.")
            : new Guardian(
                request.NewGuardian!.Name,
                request.NewGuardian.Email,
                request.NewGuardian.Phone,
                clock,
                createdByStaffId: null);

        if (createsNewGuardian)
        {
            dbContext.Guardians.Add(guardian);
        }

        Borrower borrower = new(request.Name, request.DateOfBirth, guardian.Id, clock, createdByStaffId: null);
        dbContext.Borrowers.Add(borrower);

        await dbContext.SaveChangesAsync(cancellationToken);

        return BorrowerMapper.ToResponse(borrower, guardian.Name);
    }

    public async Task<BorrowerResponse> UpdateAsync(
        Guid id, UpdateBorrowerRequest request, CancellationToken cancellationToken)
    {
        Borrower borrower = await dbContext.Borrowers.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        borrower.Rename(request.Name, clock, staffId: null);
        await dbContext.SaveChangesAsync(cancellationToken);

        Guardian guardian = await dbContext.Guardians.FirstAsync(g => g.Id == borrower.GuardianId, cancellationToken);

        return BorrowerMapper.ToResponse(borrower, guardian.Name);
    }

    /// <summary>
    /// Filtering and ordering are applied inside this query, not by chaining
    /// a further `.Where`/`.OrderBy` onto its result - EF Core cannot
    /// translate either over members of an already constructor-projected
    /// type like <see cref="BorrowerWithGuardianName"/>, see
    /// docs/adr/0017-global-exception-handler.md.
    /// </summary>
    private IQueryable<BorrowerWithGuardianName> QueryWithGuardianName(Guid? id = null) =>
        from borrower in dbContext.Borrowers
        join guardian in dbContext.Guardians on borrower.GuardianId equals guardian.Id
        where id == null || borrower.Id == id
        orderby borrower.Name
        select new BorrowerWithGuardianName(borrower, guardian.Name);

    private sealed record BorrowerWithGuardianName(Borrower Borrower, string GuardianName);
}
