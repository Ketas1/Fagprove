using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.ContactAttempts;
using SportForAlle.Api.Dtos.Loans;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class LoanService(
    AppDbContext dbContext,
    IClock clock,
    CurrentUserContext currentUser,
    FollowUpEmailSender emailSender,
    ILogger<LoanService> logger)
{
    private static readonly LoanStatus[] _openStatuses = [LoanStatus.Active, LoanStatus.Overdue];

    public async Task<IReadOnlyList<LoanResponse>> GetAllAsync(LoanStatus? status, CancellationToken cancellationToken)
    {
        List<LoanWithNames> rows = await QueryWithNames(status: status).ToListAsync(cancellationToken);

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

        Guid staffId = currentUser.RequireStaffId();
        Loan loan = new(borrower.Id, equipment.Id, request.DueDate, clock, staffId);
        equipment.MarkOnLoan(clock, staffId);

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

        Guid staffId = currentUser.RequireStaffId();
        loan.Return(clock, staffId);
        equipment.Return(request.Condition, clock, staffId);

        // "Låntakerstatus" in docs/03-domenemodell.md: a late return flags
        // the borrower, it does not block the return itself.
        if (loan.DaysLate is > 0)
        {
            borrower.RecordLateReturn(clock, staffId);
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return LoanMapper.ToResponse(loan, borrower.Name, equipment.Name);
    }

    /// <summary>The equipment was confirmed lost or destroyed while on loan - a terminal state for both.</summary>
    public async Task<LoanResponse> MarkLostAsync(Guid id, CancellationToken cancellationToken)
    {
        Loan loan = await dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utlån.");

        if (loan.Status is LoanStatus.Returned or LoanStatus.Lost)
        {
            throw new DomainConflictException("LoanAlreadyClosed", "Utlånet er allerede avsluttet.");
        }

        Equipment equipment = await dbContext.Equipment.FirstOrDefaultAsync(e => e.Id == loan.EquipmentId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        Borrower borrower = await dbContext.Borrowers.FirstOrDefaultAsync(b => b.Id == loan.BorrowerId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        Guid staffId = currentUser.RequireStaffId();
        loan.MarkLost(clock, staffId);
        equipment.MarkWrittenOff(clock, staffId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return LoanMapper.ToResponse(loan, borrower.Name, equipment.Name);
    }

    /// <summary>Business rule 6 in docs/03-domenemodell.md: every contact attempt is logged with date, method and outcome.</summary>
    public async Task<ContactAttemptResponse> LogContactAttemptAsync(
        Guid id, LogContactAttemptRequest request, CancellationToken cancellationToken)
    {
        bool loanExists = await dbContext.Loans.AnyAsync(loan => loan.Id == id, cancellationToken);

        if (!loanExists)
        {
            throw new NotFoundException("Fant ikke utlån.");
        }

        ContactAttempt attempt = new(id, request.Method, request.Outcome, clock, currentUser.RequireStaffId());
        dbContext.ContactAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ContactAttemptMapper.ToResponse(attempt);
    }

    public async Task<IReadOnlyList<ContactAttemptResponse>> GetContactAttemptsAsync(
        Guid id, CancellationToken cancellationToken)
    {
        bool loanExists = await dbContext.Loans.AnyAsync(loan => loan.Id == id, cancellationToken);

        if (!loanExists)
        {
            throw new NotFoundException("Fant ikke utlån.");
        }

        List<ContactAttempt> attempts = await dbContext.ContactAttempts
            .Where(attempt => attempt.LoanId == id)
            .OrderByDescending(attempt => attempt.CreatedAt)
            .ToListAsync(cancellationToken);

        return attempts.Select(ContactAttemptMapper.ToResponse).ToList();
    }

    /// <summary>
    /// Sends the overdue follow-up email via EmailJS and, only on success,
    /// logs it as a <see cref="ContactAttempt"/> (business rule 6 in
    /// docs/03-domenemodell.md) - one atomic action, not two. If the email
    /// fails to send, nothing is logged: a contact attempt records that
    /// contact actually happened, not that it was merely attempted. See
    /// docs/adr/0022-emailjs-server-side.md.
    /// </summary>
    public async Task<ContactAttemptResponse> SendFollowUpEmailAsync(Guid id, CancellationToken cancellationToken)
    {
        Loan loan = await dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utlån.");

        LoanRules.EnsureLoanIsOverdue(loan, clock);

        Borrower borrower = await dbContext.Borrowers.FirstOrDefaultAsync(b => b.Id == loan.BorrowerId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke låntaker.");

        Guardian guardian = await dbContext.Guardians.FirstOrDefaultAsync(g => g.Id == borrower.GuardianId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke foresatt.");

        Equipment equipment = await dbContext.Equipment.FirstOrDefaultAsync(e => e.Id == loan.EquipmentId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        // Same calendar-date arithmetic as Loan.Return()'s DaysLate - see the
        // 2026-09-07 addendum to ADR-0011.
        int daysOverdue = (clock.UtcNow.UtcDateTime.Date - loan.DueDate.UtcDateTime.Date).Days;

        Dictionary<string, string> templateParams = new()
        {
            ["email"] = guardian.Email,
            ["to_name"] = guardian.Name,
            ["child_name"] = borrower.Name,
            ["equipment_name"] = equipment.Name,
            ["due_date"] = loan.DueDate.ToString("dd.MM.yyyy"),
            ["days_overdue"] = daysOverdue.ToString(),
        };

        try
        {
            await emailSender.SendAsync(templateParams, cancellationToken);
        }
        catch (EmailSendException)
        {
            // The exception's own message (EmailJS's raw response text) is
            // not logged here - it could echo request content back, and
            // logs must never carry personal data, see CLAUDE.md.
            logger.LogWarning("Follow-up email failed to send for loan {LoanId}.", id);
            throw new DomainConflictException(
                "EmailSendFailed", "E-posten kunne ikke sendes.", StatusCodes.Status502BadGateway);
        }

        Guid staffId = currentUser.RequireStaffId();
        ContactAttempt attempt = new(
            id, ContactMethod.Email, "E-post sendt automatisk via oppfølgingsknapp.", clock, staffId);
        dbContext.ContactAttempts.Add(attempt);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ContactAttemptMapper.ToResponse(attempt);
    }

    /// <summary>
    /// The write-time half of ADR-0011: finds every loan still marked <see cref="LoanStatus.Active"/>
    /// but past its due date and materialises it to <see cref="LoanStatus.Overdue"/>. Called by
    /// <see cref="BackgroundJobs.OverdueLoanBackgroundService"/> on a timer, and directly from
    /// tests via an injected clock - never depends on the timer itself having run.
    /// </summary>
    public async Task<int> RefreshOverdueLoansAsync(CancellationToken cancellationToken)
    {
        List<Loan> dueLoans = await dbContext.Loans
            .Where(loan => loan.Status == LoanStatus.Active && loan.DueDate < clock.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (Loan loan in dueLoans)
        {
            loan.RefreshOverdueStatus(clock);
        }

        if (dueLoans.Count > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return dueLoans.Count;
    }

    /// <summary>
    /// Filtering and ordering are applied inside this query, not by chaining
    /// a further `.Where`/`.OrderByDescending` onto its result - EF Core
    /// cannot translate either over members of an already
    /// constructor-projected type like <see cref="LoanWithNames"/>, see
    /// docs/adr/0017-global-exception-handler.md.
    /// </summary>
    private IQueryable<LoanWithNames> QueryWithNames(Guid? id = null, LoanStatus? status = null) =>
        from loan in dbContext.Loans
        join borrower in dbContext.Borrowers on loan.BorrowerId equals borrower.Id
        join equipment in dbContext.Equipment on loan.EquipmentId equals equipment.Id
        where (id == null || loan.Id == id) && (status == null || loan.Status == status)
        orderby loan.StartedAt descending
        select new LoanWithNames(loan, borrower.Name, equipment.Name);

    private sealed record LoanWithNames(Loan Loan, string BorrowerName, string EquipmentName);
}
