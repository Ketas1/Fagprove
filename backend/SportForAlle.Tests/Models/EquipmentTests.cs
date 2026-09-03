using SportForAlle.Api.Models;
using SportForAlle.Tests.TestSupport;

namespace SportForAlle.Tests.Models;

public class EquipmentTests
{
    private static readonly FakeClock _clock = new();
    private static readonly Guid _categoryId = Guid.NewGuid();

    private static Equipment CreateEquipment() =>
        new("Alpinski str. 140", "SN-001", _categoryId, EquipmentCondition.Good, _clock, createdByStaffId: null);

    [Fact]
    public void Constructor_starts_available()
    {
        Equipment equipment = CreateEquipment();

        Assert.Equal(EquipmentStatus.Available, equipment.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_blank_serial_number(string serialNumber)
    {
        Assert.Throws<ArgumentException>(
            () => new Equipment("Ski", serialNumber, _categoryId, EquipmentCondition.Good, _clock, null));
    }

    [Fact]
    public void MarkOnLoan_moves_available_equipment_to_on_loan()
    {
        Equipment equipment = CreateEquipment();

        equipment.MarkOnLoan(_clock, Guid.NewGuid());

        Assert.Equal(EquipmentStatus.OnLoan, equipment.Status);
    }

    [Fact]
    public void MarkOnLoan_rejects_equipment_that_is_already_on_loan()
    {
        Equipment equipment = CreateEquipment();
        equipment.MarkOnLoan(_clock, Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => equipment.MarkOnLoan(_clock, Guid.NewGuid()));
    }

    [Fact]
    public void Return_in_good_condition_makes_the_equipment_available_again()
    {
        Equipment equipment = CreateEquipment();
        equipment.MarkOnLoan(_clock, Guid.NewGuid());

        equipment.Return(EquipmentCondition.Good, _clock, Guid.NewGuid());

        Assert.Equal(EquipmentStatus.Available, equipment.Status);
        Assert.Equal(EquipmentCondition.Good, equipment.Condition);
    }

    [Fact]
    public void Return_damaged_takes_the_equipment_out_of_service()
    {
        Equipment equipment = CreateEquipment();
        equipment.MarkOnLoan(_clock, Guid.NewGuid());

        equipment.Return(EquipmentCondition.Damaged, _clock, Guid.NewGuid());

        Assert.Equal(EquipmentStatus.OutOfService, equipment.Status);
    }

    [Fact]
    public void Return_rejects_equipment_that_is_not_on_loan()
    {
        Equipment equipment = CreateEquipment();

        Assert.Throws<InvalidOperationException>(
            () => equipment.Return(EquipmentCondition.Good, _clock, Guid.NewGuid()));
    }

    [Fact]
    public void Repair_moves_out_of_service_equipment_back_to_available()
    {
        Equipment equipment = CreateEquipment();
        equipment.MarkOnLoan(_clock, Guid.NewGuid());
        equipment.Return(EquipmentCondition.Damaged, _clock, Guid.NewGuid());

        equipment.Repair(_clock, Guid.NewGuid());

        Assert.Equal(EquipmentStatus.Available, equipment.Status);
    }

    [Fact]
    public void Repair_rejects_equipment_that_is_not_out_of_service()
    {
        Equipment equipment = CreateEquipment();

        Assert.Throws<InvalidOperationException>(() => equipment.Repair(_clock, Guid.NewGuid()));
    }

    [Fact]
    public void MarkWrittenOff_is_terminal()
    {
        Equipment equipment = CreateEquipment();
        equipment.MarkOnLoan(_clock, Guid.NewGuid());

        equipment.MarkWrittenOff(_clock, Guid.NewGuid());

        Assert.Equal(EquipmentStatus.WrittenOff, equipment.Status);
        Assert.Throws<InvalidOperationException>(() => equipment.Repair(_clock, Guid.NewGuid()));
    }

    [Fact]
    public void MarkWrittenOff_rejects_equipment_that_is_not_on_loan()
    {
        Equipment equipment = CreateEquipment();

        Assert.Throws<InvalidOperationException>(() => equipment.MarkWrittenOff(_clock, Guid.NewGuid()));
    }
}
