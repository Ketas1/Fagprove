namespace SportForAlle.Api.Dtos.Reports;

public record PopularEquipmentItem(Guid EquipmentId, string EquipmentName, string CategoryName, int LoanCount);

public record PopularEquipmentReportResponse(IReadOnlyList<PopularEquipmentItem> Items);
