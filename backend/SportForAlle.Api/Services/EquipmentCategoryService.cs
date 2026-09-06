using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.EquipmentCategories;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
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
        if (request.ParentCategoryId.HasValue)
        {
            bool parentExists = await dbContext.EquipmentCategories
                .AnyAsync(category => category.Id == request.ParentCategoryId, cancellationToken);

            if (!parentExists)
            {
                throw new NotFoundException("Fant ikke overordnet kategori.");
            }
        }

        await EnsureNameIsUniqueUnderParent(request.Name, request.ParentCategoryId, excludingId: null, cancellationToken);

        EquipmentCategory category = new(request.Name, clock, currentUser.RequireStaffId(), request.ParentCategoryId);

        dbContext.EquipmentCategories.Add(category);
        await dbContext.SaveChangesAsync(cancellationToken);

        return EquipmentCategoryMapper.ToResponse(category);
    }

    public async Task<EquipmentCategoryResponse> RenameAsync(
        Guid id, RenameEquipmentCategoryRequest request, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await dbContext.EquipmentCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke kategori.");

        await EnsureNameIsUniqueUnderParent(request.Name, category.ParentCategoryId, excludingId: id, cancellationToken);

        category.Rename(request.Name, clock, currentUser.RequireStaffId());
        await dbContext.SaveChangesAsync(cancellationToken);

        return EquipmentCategoryMapper.ToResponse(category);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await dbContext.EquipmentCategories
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke kategori.");

        bool hasSubcategories = await dbContext.EquipmentCategories
            .AnyAsync(c => c.ParentCategoryId == id, cancellationToken);
        bool hasEquipment = await dbContext.Equipment
            .AnyAsync(e => e.CategoryId == id, cancellationToken);

        EquipmentCategoryRules.EnsureCanBeDeleted(hasSubcategories, hasEquipment);

        dbContext.EquipmentCategories.Remove(category);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameIsUniqueUnderParent(
        string name, Guid? parentCategoryId, Guid? excludingId, CancellationToken cancellationToken)
    {
        bool duplicateName = await dbContext.EquipmentCategories
            .AnyAsync(
                category => category.Id != excludingId
                    && category.ParentCategoryId == parentCategoryId
                    && category.Name == name,
                cancellationToken);

        if (duplicateName)
        {
            throw new DomainConflictException("DuplicateCategoryName", "En kategori med dette navnet finnes allerede her.");
        }
    }
}
