using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class LoanService(AppDbContext dbContext, IClock clock)
{
    private static readonly LoanStatus[] _openStatuses = [LoanStatus.Active, LoanStatus.Overdue];

    public async Task<IReadOnlyList<LoanResponse>> GetAllAsync(LoanStatus? status, CancellationToken cancellationToken)
    {
        List<LoanWithNames> rows = await QueryWithNames(status: status)
            .OrderByDescending(row => row.Loan.StartedAt)
            .ToListAsync(cancellationToken);

        return rows.Select(row => LoanMapper.ToResponse(row.Loan, row.BorrowerName, row.EquipmentName)).ToList();
    }

    public async Task<LoanResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        LoanWithNames row = await QueryWithNames(id: id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Fant ikke utlån.");

        return LoanMapper.ToResponse(row.Loan, row.BorrowerName, row.EquipmentName);
    }

    public async Task<LoanResponse> RegisterAsync(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        Borrower borrower = await dbContext.Borrowers
            .FirstOrDefaultAsync(b => b.Id == request.BorrowerId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        Equipment equipment = await dbContext.Equipment
            .FirstOrDefaultAsync(e => e.Id == request.EquipmentId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        List<Loan> openLoans = await dbContext.Loans
            .Where(loan => loan.BorrowerId == borrower.Id && _openStatuses.Contains(loan.Status))
            .ToListAsync(cancellationToken);

        LoanRules.EnsureBorrowerCanBorrow(borrower, openLoans, clock);
        LoanRules.EnsureEquipmentAvailable(equipment);

        Loan loan = new(borrower.Id, equipment.Id, request.DueDate, clock, createdByStaffId: null);
        equipment.MarkOnLoan(clock, staffId: null);

        dbContext.Loans.Add(loan);
        await dbContext.SaveChangesAsync(cancellationToken);

        return LoanMapper.ToResponse(loan, borrower.Name, equipment.Name);
    }

    public async Task<LoanResponse> ReturnAsync(Guid id, ReturnLoanRequest request, CancellationToken cancellationToken)
    {
        Loan loan = await dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utlån.");

        if (loan.Status is LoanStatus.Returned or LoanStatus.Lost)
        {
            throw new DomainConflictException("LoanAlreadyClosed", "Utlånet er allerede avsluttet.");
        }

        Equipment equipment = await dbContext.Equipment
            .FirstOrDefaultAsync(e => e.Id == loan.EquipmentId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        Borrower borrower = await dbContext.Borrowers
            .FirstOrDefaultAsync(b => b.Id == loan.BorrowerId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        loan.Return(clock, staffId: null);
        equipment.Return(request.Condition, clock, staffId: null);

        // "Låntakerstatus" in docs/03-domenemodell.md: a late return flags
        // the borrower, it does not block the return itself.
        if (loan.DaysLate is > 0)
        {
            borrower.RecordLateReturn(clock, staffId: null);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return LoanMapper.ToResponse(loan, borrower.Name, equipment.Name);
    }

    /// <summary>
    /// Filters are applied inside this query, not by chaining a further
    /// `.Where` onto its result - EF Core cannot translate a predicate over
    /// members of an already constructor-projected type like
    /// <see cref="LoanWithNames"/>.
    /// </summary>
    private IQueryable<LoanWithNames> QueryWithNames(Guid? id = null, LoanStatus? status = null) =>
        from loan in dbContext.Loans
        join borrower in dbContext.Borrowers on loan.BorrowerId equals borrower.Id
        join equipment in dbContext.Equipment on loan.EquipmentId equals equipment.Id
        where (id == null || loan.Id == id) && (status == null || loan.Status == status)
        select new LoanWithNames(loan, borrower.Name, equipment.Name);

    private sealed record LoanWithNames(Loan Loan, string BorrowerName, string EquipmentName);
}
