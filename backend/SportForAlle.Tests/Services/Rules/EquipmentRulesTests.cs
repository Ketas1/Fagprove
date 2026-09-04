using SportForAlle.Api.Services.Rules;
using SportForAlle.Api.Validation;

namespace SportForAlle.Tests.Services.Rules;

public class EquipmentRulesTests
{
    [Fact]
    public void EnsureCanBeDeleted_allows_equipment_with_no_loan_history()
    {
        EquipmentRules.EnsureCanBeDeleted(hasLoanHistory: false);
    }

    [Fact]
    public void EnsureCanBeDeleted_rejects_equipment_with_loan_history()
    {
        DomainConflictException exception = Assert.Throws<DomainConflictException>(
            () => EquipmentRules.EnsureCanBeDeleted(hasLoanHistory: true));

        Assert.Equal("EquipmentHasLoanHistory", exception.Reason);
    }
}
