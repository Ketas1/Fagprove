using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Bans;
using SportForAlle.Api.Dtos.Borrowers;
using SportForAlle.Api.Dtos.Notes;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class BorrowerService(AppDbContext dbContext, IClock clock, CurrentUserContext currentUser)
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

        Guid staffId = currentUser.RequireStaffId();

        Guardian guardian = linksExistingGuardian
            ? await dbContext.Guardians.FirstOrDefaultAsync(g => g.Id == request.GuardianId, cancellationToken)
                ?? throw new NotFoundException("Fant ikke foresatt.")
            : new Guardian(
                request.NewGuardian!.Name,
                request.NewGuardian.Email,
                request.NewGuardian.Phone,
                clock,
                staffId,
                request.NewGuardian.IdentityVerified);

        if (createsNewGuardian)
        {
            dbContext.Guardians.Add(guardian);
        }

        Borrower borrower = new(request.Name, request.DateOfBirth, guardian.Id, clock, staffId);
        dbContext.Borrowers.Add(borrower);

        await dbContext.SaveChangesAsync(cancellationToken);

        return BorrowerMapper.ToResponse(borrower, guardian.Name);
    }

    public async Task<BorrowerResponse> UpdateAsync(
        Guid id, UpdateBorrowerRequest request, CancellationToken cancellationToken)
    {
        Borrower borrower = await dbContext.Borrowers.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        borrower.Rename(request.Name, clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        Guardian guardian = await dbContext.Guardians.FirstAsync(g => g.Id == borrower.GuardianId, cancellationToken);

        return BorrowerMapper.ToResponse(borrower, guardian.Name);
    }

    /// <summary>Business rule 2 in docs/03-domenemodell.md: a banned borrower cannot register a new loan.</summary>
    public async Task<BanResponse> BanAsync(Guid id, BanBorrowerRequest request, CancellationToken cancellationToken)
    {
        Borrower borrower = await dbContext.Borrowers.FirstOrDefaultAsync(b => b.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        if (borrower.Status == BorrowerStatus.Banned)
        {
            throw new DomainConflictException("BorrowerAlreadyBanned", "Låntakeren er allerede utestengt.");
        }

        borrower.Ban(request.Reason, clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return BanMapper.ToResponse(borrower.Bans.Single(ban => ban.IsActive));
    }

    /// <summary>Records the fee that business rule 8 requires before a ban can be lifted.</summary>
    public async Task<BanResponse> RecordBanFeePaidAsync(Guid id, CancellationToken cancellationToken)
    {
        Borrower borrower = LoadForBanTransition(await dbContext.Borrowers
            .Include(b => b.Bans)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken));

        borrower.RecordBanFeePaid(clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return BanMapper.ToResponse(CurrentBan(borrower));
    }

    /// <summary>Business rule 8 in docs/03-domenemodell.md: requires the fee to already be recorded as paid.</summary>
    public async Task LiftBanAsync(Guid id, CancellationToken cancellationToken)
    {
        Borrower borrower = LoadForBanTransition(await dbContext.Borrowers
            .Include(b => b.Bans)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken));

        if (CurrentBan(borrower).FeePaidAt is null)
        {
            throw new DomainConflictException("BanFeeNotPaid", "Gebyret er ikke registrert betalt ennå.");
        }

        borrower.LiftBan(clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NoteResponse> AddNoteAsync(Guid id, CreateNoteRequest request, CancellationToken cancellationToken)
    {
        bool borrowerExists = await dbContext.Borrowers.AnyAsync(b => b.Id == id, cancellationToken);

        if (!borrowerExists)
        {
            throw new NotFoundException("Fant ikke låntaker.");
        }

        Note note = new(id, request.Text, clock, currentUser.RequireStaffId());
        dbContext.Notes.Add(note);
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoteMapper.ToResponse(note);
    }

    public async Task<IReadOnlyList<NoteResponse>> GetNotesAsync(Guid id, CancellationToken cancellationToken)
    {
        bool borrowerExists = await dbContext.Borrowers.AnyAsync(b => b.Id == id, cancellationToken);

        if (!borrowerExists)
        {
            throw new NotFoundException("Fant ikke låntaker.");
        }

        List<Note> notes = await dbContext.Notes
            .Where(note => note.BorrowerId == id)
            .OrderByDescending(note => note.CreatedAt)
            .ToListAsync(cancellationToken);

        return notes.Select(NoteMapper.ToResponse).ToList();
    }

    /// <summary>Shared not-banned guard for the two transitions that require an existing, active ban.</summary>
    private static Borrower LoadForBanTransition(Borrower? borrower)
    {
        if (borrower is null)
        {
            throw new NotFoundException("Fant ikke låntaker.");
        }

        if (borrower.Status != BorrowerStatus.Banned)
        {
            throw new DomainConflictException("BorrowerNotBanned", "Låntakeren er ikke utestengt.");
        }

        return borrower;
    }

    private static Ban CurrentBan(Borrower borrower) => borrower.Bans.Single(ban => ban.IsActive);

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
