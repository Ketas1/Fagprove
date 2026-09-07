namespace SportForAlle.Api.Dtos.Reports;

public record LoanCountReportResponse(DateOnly From, DateOnly To, int TotalLoans);
