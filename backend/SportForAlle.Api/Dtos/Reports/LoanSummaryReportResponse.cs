namespace SportForAlle.Api.Dtos.Reports;

/// <summary>
/// The five figures both reports share, see "Rapportering" in
/// docs/03-domenemodell.md. Every figure counts loans whose
/// <c>Loan.StartedAt</c> falls in the period, and <see cref="LoanStatus"/> has
/// exactly the four values covered below, so
/// <see cref="ReturnedOnTime"/> + <see cref="ReturnedLate"/> +
/// <see cref="NotReturned"/> + <see cref="StillActive"/> always equals
/// <see cref="TotalLoans"/>. A reader can check the arithmetic themselves,
/// which is the point.
/// </summary>
/// <param name="TotalLoans">Loans registered in the period.</param>
/// <param name="ReturnedOnTime">Returned within the due date.</param>
/// <param name="ReturnedLate">Returned after the due date.</param>
/// <param name="NotReturned">Overdue or confirmed lost.</param>
/// <param name="StillActive">Running, and not yet past the due date.</param>
public record LoanFigures(
    int TotalLoans,
    int ReturnedOnTime,
    int ReturnedLate,
    int NotReturned,
    int StillActive);

/// <summary>
/// Report 1 of 2. <c>From</c>/<c>To</c> are null when the caller asked for the
/// whole history rather than a period.
/// </summary>
public record LoanSummaryReportResponse(DateOnly? From, DateOnly? To, LoanFigures Figures);
