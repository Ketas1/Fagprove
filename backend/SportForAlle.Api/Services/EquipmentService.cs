using Microsoft.EntityFrameworkCore;
using SportForAlle.Api.Data;
using SportForAlle.Api.Dtos.Equipment;
using SportForAlle.Api.Helpers;
using SportForAlle.Api.Mapping;
using SportForAlle.Api.Models;
using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services;

public class EquipmentService(AppDbContext dbContext, IClock clock)
{
    public async Task<IReadOnlyList<EquipmentResponse>> GetAllAsync(
        EquipmentStatus? status, Guid? categoryId, CancellationToken cancellationToken)
    {
        List<EquipmentWithCategoryName> rows = await QueryWithCategoryName(status: status, categoryId: categoryId)
            .ToListAsync(cancellationToken);

        return rows.Select(row => EquipmentMapper.ToResponse(row.Equipment, row.CategoryName)).ToList();
    }

    public async Task<EquipmentResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        EquipmentWithCategoryName row = await QueryWithCategoryName(id: id).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        return EquipmentMapper.ToResponse(row.Equipment, row.CategoryName);
    }

    public async Task<EquipmentResponse> CreateAsync(CreateEquipmentRequest request, CancellationToken cancellationToken)
    {
        EquipmentCategory category = await dbContext.EquipmentCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke kategori.");

        bool duplicateSerialNumber = await dbContext.Equipment
            .AnyAsync(equipment => equipment.SerialNumber == request.SerialNumber, cancellationToken);

        if (duplicateSerialNumber)
        {
            throw new DomainConflictException("DuplicateSerialNumber", "Serienummeret er allerede i bruk.");
        }

        Equipment equipment = new(
            request.Name, request.SerialNumber, request.CategoryId, request.Condition, clock, createdByStaffId: null);

        dbContext.Equipment.Add(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return EquipmentMapper.ToResponse(equipment, category.Name);
    }

    public async Task<EquipmentResponse> UpdateAsync(
        Guid id, UpdateEquipmentRequest request, CancellationToken cancellationToken)
    {
        Equipment equipment = await dbContext.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        EquipmentCategory category = await dbContext.EquipmentCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId, cancellationToken)
            ?? throw new NotFoundException("Fant ikke kategori.");

        equipment.Rename(request.Name, clock, staffId: null);
        equipment.Recategorize(request.CategoryId, clock, staffId: null);

        await dbContext.SaveChangesAsync(cancellationToken);

        return EquipmentMapper.ToResponse(equipment, category.Name);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        Equipment equipment = await dbContext.Equipment.FirstOrDefaultAsync(e => e.Id == id, cancellationToken)
            ?? throw new NotFoundException("Fant ikke utstyr.");

        bool hasLoanHistory = await dbContext.Loans.AnyAsync(loan => loan.EquipmentId == id, cancellationToken);
        EquipmentRules.EnsureCanBeDeleted(hasLoanHistory);

        dbContext.Equipment.Remove(equipment);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Filtering and ordering are applied inside this query, not by chaining
    /// a further `.Where`/`.OrderBy` onto its result - EF Core cannot
    /// translate either over members of an already constructor-projected
    /// type like <see cref="EquipmentWithCategoryName"/>, see
    /// docs/adr/0017-global-exception-handler.md.
    /// </summary>
    private IQueryable<EquipmentWithCategoryName> QueryWithCategoryName(
        Guid? id = null, EquipmentStatus? status = null, Guid? categoryId = null) =>
        from equipment in dbContext.Equipment
        join category in dbContext.EquipmentCategories on equipment.CategoryId equals category.Id
        where (id == null || equipment.Id == id)
            && (status == null || equipment.Status == status)
            && (categoryId == null || equipment.CategoryId == categoryId)
        orderby equipment.Name
        select new EquipmentWithCategoryName(equipment, category.Name);

    private sealed record EquipmentWithCategoryName(Equipment Equipment, string CategoryName);
}
