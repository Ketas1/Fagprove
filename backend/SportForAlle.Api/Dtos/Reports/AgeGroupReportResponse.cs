namespace SportForAlle.Api.Dtos.Reports;

/// <summary>The fixed age groups from docs/03-domenemodell.md - 3-6, 7-12, 13-18.</summary>
public record AgeGroupCount(string AgeGroup, int Count);

public record AgeGroupReportResponse(DateOnly From, DateOnly To, IReadOnlyList<AgeGroupCount> Groups);
