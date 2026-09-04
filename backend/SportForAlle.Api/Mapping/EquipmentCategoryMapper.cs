using SportForAlle.Api.Dtos.EquipmentCategories;
using SportForAlle.Api.Models;

namespace SportForAlle.Api.Mapping;

public static class EquipmentCategoryMapper
{
    public static EquipmentCategoryResponse ToResponse(EquipmentCategory category) =>
        new(category.Id, category.Name, category.CreatedByStaffId, category.UpdatedByStaffId);
}
