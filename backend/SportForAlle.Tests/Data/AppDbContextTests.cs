using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SportForAlle.Api.Data;
using SportForAlle.Api.Models;

namespace SportForAlle.Tests.Data;

/// <summary>
/// Builds the EF model without touching a database. A broken
/// IEntityTypeConfiguration fails here instead of at the first query.
/// </summary>
public class AppDbContextTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=model-validation-only")
            .Options;

        return new AppDbContext(options);
    }

    public static TheoryData<Type, string> EntitiesAndTheirTables => new()
    {
        { typeof(EquipmentCategory), "EquipmentCategories" },
        { typeof(Staff), "Staff" },
        { typeof(Guardian), "Guardians" },
        { typeof(Borrower), "Borrowers" },
        { typeof(Equipment), "Equipment" },
        { typeof(Loan), "Loans" },
        { typeof(ContactAttempt), "ContactAttempts" },
        { typeof(Note), "Notes" },
        { typeof(Ban), "Bans" },
    };

    [Theory]
    [MemberData(nameof(EntitiesAndTheirTables))]
    public void Model_maps_every_domain_entity_to_its_table(Type entityClrType, string expectedTableName)
    {
        using AppDbContext context = CreateContext();

        IEntityType? entityType = context.Model.FindEntityType(entityClrType);

        Assert.NotNull(entityType);
        Assert.Equal(expectedTableName, entityType.GetTableName());
    }

    [Theory]
    [MemberData(nameof(EntitiesAndTheirTables))]
    public void Every_entity_carries_the_shared_audit_columns(Type entityClrType, string _)
    {
        using AppDbContext context = CreateContext();
        IEntityType entityType = context.Model.FindEntityType(entityClrType)!;

        Assert.NotNull(entityType.FindProperty(nameof(AuditableEntity.CreatedAt)));
        Assert.NotNull(entityType.FindProperty(nameof(AuditableEntity.CreatedByStaffId)));
        Assert.NotNull(entityType.FindProperty(nameof(AuditableEntity.UpdatedAt)));
        Assert.NotNull(entityType.FindProperty(nameof(AuditableEntity.UpdatedByStaffId)));
    }

    [Fact]
    public void Configurations_in_the_assembly_are_applied()
    {
        using AppDbContext context = CreateContext();

        IEntityType entityType = context.Model.FindEntityType(typeof(EquipmentCategory))!;
        IProperty name = entityType.FindProperty(nameof(EquipmentCategory.Name))!;

        // These come from EquipmentCategoryConfiguration, so if the assembly scan
        // in OnModelCreating stops working, this fails.
        Assert.False(name.IsNullable);
        Assert.Equal(EquipmentCategory.NameMaxLength, name.GetMaxLength());
    }

    [Fact]
    public void Enums_are_stored_as_text_not_numbers()
    {
        using AppDbContext context = CreateContext();

        IEntityType loanType = context.Model.FindEntityType(typeof(Loan))!;
        IProperty status = loanType.FindProperty(nameof(Loan.Status))!;

        Assert.Equal(typeof(string), status.GetProviderClrType());
    }

    [Fact]
    public void Equipment_serial_number_is_unique()
    {
        using AppDbContext context = CreateContext();

        IEntityType equipmentType = context.Model.FindEntityType(typeof(Equipment))!;
        IIndex? index = equipmentType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Equipment.SerialNumber)]));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
    }

    [Fact]
    public void A_borrower_cannot_have_more_than_one_active_ban()
    {
        using AppDbContext context = CreateContext();

        IEntityType banType = context.Model.FindEntityType(typeof(Ban))!;
        IIndex? index = banType.GetIndexes()
            .SingleOrDefault(i => i.Properties.Select(p => p.Name).SequenceEqual([nameof(Ban.BorrowerId)]));

        Assert.NotNull(index);
        Assert.True(index.IsUnique);
        Assert.Contains("LiftedAt", index.GetFilter());
    }

    [Fact]
    public void Deleting_a_guardian_with_borrowers_is_restricted_not_cascaded()
    {
        using AppDbContext context = CreateContext();

        IEntityType borrowerType = context.Model.FindEntityType(typeof(Borrower))!;
        IForeignKey guardianForeignKey = borrowerType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Guardian));

        Assert.Equal(DeleteBehavior.Restrict, guardianForeignKey.DeleteBehavior);
    }

    [Fact]
    public void Deleting_a_borrower_or_equipment_with_loan_history_is_restricted()
    {
        using AppDbContext context = CreateContext();

        IEntityType loanType = context.Model.FindEntityType(typeof(Loan))!;
        IForeignKey[] historyForeignKeys =
        [
            .. loanType.GetForeignKeys()
                .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Borrower)
                    || fk.PrincipalEntityType.ClrType == typeof(Equipment)),
        ];

        Assert.Equal(2, historyForeignKeys.Length);
        Assert.All(historyForeignKeys, fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
    }

    [Fact]
    public void Removing_a_staff_member_sets_audit_references_to_null_instead_of_blocking()
    {
        using AppDbContext context = CreateContext();

        IEntityType loanType = context.Model.FindEntityType(typeof(Loan))!;
        IForeignKey createdByForeignKey = loanType.GetForeignKeys()
            .Single(fk => fk.Properties.Select(p => p.Name).SequenceEqual([nameof(AuditableEntity.CreatedByStaffId)]));

        Assert.Equal(DeleteBehavior.SetNull, createdByForeignKey.DeleteBehavior);
    }
}
