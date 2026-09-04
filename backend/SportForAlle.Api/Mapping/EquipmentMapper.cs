using SportForAlle.Api.Dtos.Equipment;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class EquipmentMapper
{
    /// <summary>
    /// <paramref name="categoryName"/> is passed in rather than read from a
    /// navigation property - <see cref="Equipment"/> has none, see
    /// docs/04-databasedesign.md. Callers join it in themselves.
    /// </summary>
    public static EquipmentResponse ToResponse(Equipment equipment, string categoryName) =>
        new(
            equipment.Id,
            equipment.Name,
            equipment.SerialNumber,
            equipment.CategoryId,
            categoryName,
            equipment.Condition,
            equipment.Status,
            equipment.CreatedByStaffId,
            equipment.UpdatedByStaffId);
}
