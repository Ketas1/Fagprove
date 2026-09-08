namespace SportForAlle.Api.Dtos.Reports;

/// <summary>The fixed age groups from docs/03-domenemodell.md - 3-7, 8-12, 13-18.</summary>
public record AgeGroupFigures(string AgeGroup, LoanFigures Figures);

/// <summary>
/// Report 2 of 2 - the same five figures as
/// <see cref="LoanSummaryReportResponse"/>, split by the borrower's age at
/// <c>Loan.StartedAt</c>. Summing one figure across the three groups gives the
/// same number the summary report returns for that figure.
/// </summary>
public record AgeGroupReportResponse(DateOnly? From, DateOnly? To, IReadOnlyList<AgeGroupFigures> Groups);
