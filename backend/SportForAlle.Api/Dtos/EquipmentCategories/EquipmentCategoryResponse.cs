namespace SportForAlle.Api.Dtos.EquipmentCategories;

public record EquipmentCategoryResponse(
    Guid Id,
    string Name,
    Guid? ParentCategoryId,
    Guid? CreatedByStaffId,
    Guid? UpdatedByStaffId);
