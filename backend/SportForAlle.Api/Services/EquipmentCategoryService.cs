using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.EquipmentCategories;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class EquipmentCategoryService(AppDbContext dbContext, IClock clock, CurrentUserContext currentUser)
{
    public async Task<IReadOnlyList<EquipmentCategoryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        List<EquipmentCategory> categories = await dbContext.EquipmentCategories
            .OrderBy(category => category.Name)
            .ToListAsync(cancellationToken);

        return categories.Select(EquipmentCategoryMapper.ToResponse).ToList();
    }

    public async Task<EquipmentCategoryResponse> CreateAsync(
        CreateEquipmentCategoryRequest request, CancellationToken cancellationToken)
    {
        bool duplicateName = await dbContext.EquipmentCategories
            .AnyAsync(category => category.Name == request.Name, cancellationToken);

        if (duplicateName)
        {
            throw new DomainConflictException("DuplicateCategoryName", "En kategori med dette navnet finnes allerede.");
        }

        EquipmentCategory category = new(request.Name, clock, currentUser.RequireStaffId());

        dbContext.EquipmentCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return EquipmentCategoryMapper.ToResponse(category);
    }
}
