namespace SportForAlle.Api.Dtos.Reports;

/// <summary>
/// The two "Rapportering" rows in docs/03-domenemodell.md that share this one
/// endpoint: <see cref="LateReturnedCount"/> is "Forsene leveringer"
/// (<c>Loan.DaysLate &gt; 0</c>), <see cref="UndeliveredCount"/> is "Uleverte
/// lån" (<c>Loan.Status</c> is <c>Overdue</c> or <c>Lost</c>). Aggregated
/// counts only, never individual loans or borrowers - see the reporting GDPR
/// requirement in docs/09-lover-og-regler.md.
/// </summary>
public record OverdueSummaryReportResponse(int LateReturnedCount, int UndeliveredCount);
