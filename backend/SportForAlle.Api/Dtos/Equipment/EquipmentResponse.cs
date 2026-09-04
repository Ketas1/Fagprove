using SportForAlle.Api.Models;

namespace SportForAlle.Api.Dtos.Equipment;

public record EquipmentResponse(
    Guid Id,
    string Name,
    string SerialNumber,
    Guid CategoryId,
    string CategoryName,
    EquipmentCondition Condition,
    EquipmentStatus Status);
