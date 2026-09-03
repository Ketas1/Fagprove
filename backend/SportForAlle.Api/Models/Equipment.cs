using SportForAlle.Api.Helpers;

namespace SportForAlle.Api.Models;

/// <summary>
/// A physical item that can be lent out. See the "Utstyrstatus" state machine
/// in docs/03-domenemodell.md - every transition below is one of its arrows.
/// </summary>
public class Equipment : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int SerialNumberMaxLength = 100;

    /// <summary>Required by EF Core, which materialises entities without a public constructor.</summary>
    private Equipment()
    {
    }

    public Equipment(
        string name,
        string serialNumber,
        Guid categoryId,
        EquipmentCondition condition,
        IClock clock,
        Guid? createdByStaffId)
        : base(clock, createdByStaffId)
    {
        Name = ValidateName(name);
        SerialNumber = ValidateSerialNumber(serialNumber);
        CategoryId = categoryId;
        Condition = condition;
        Status = EquipmentStatus.Available;
    }

    public string Name { get; private set; } = string.Empty;

    public string SerialNumber { get; private set; } = string.Empty;

    public Guid CategoryId { get; private set; }

    public EquipmentCondition Condition { get; private set; }

    public EquipmentStatus Status { get; private set; }

    public void Rename(string name, IClock clock, Guid? staffId)
    {
        Name = ValidateName(name);
        Touch(clock, staffId);
    }

    public void Recategorize(Guid categoryId, IClock clock, Guid? staffId)
    {
        CategoryId = categoryId;
        Touch(clock, staffId);
    }

    /// <summary>Marks the item lent out. Only <see cref="EquipmentStatus.Available"/> equipment can be loaned.</summary>
    public void MarkOnLoan(IClock clock, Guid? staffId)
    {
        RequireStatus(EquipmentStatus.OnLoan, EquipmentStatus.Available);
        Status = EquipmentStatus.OnLoan;
        Touch(clock, staffId);
    }

    /// <summary>
    /// The item was returned. A <see cref="EquipmentCondition.Damaged"/> item goes
    /// out of service for repair; anything else becomes available again.
    /// </summary>
    public void Return(EquipmentCondition condition, IClock clock, Guid? staffId)
    {
        RequireStatus(EquipmentStatus.Available, EquipmentStatus.OnLoan);
        Condition = condition;
        Status = condition == EquipmentCondition.Damaged ? EquipmentStatus.OutOfService : EquipmentStatus.Available;
        Touch(clock, staffId);
    }

    /// <summary>The item has been repaired and can be loaned out again.</summary>
    public void Repair(IClock clock, Guid? staffId)
    {
        RequireStatus(EquipmentStatus.Available, EquipmentStatus.OutOfService);
        Condition = EquipmentCondition.Good;
        Status = EquipmentStatus.Available;
        Touch(clock, staffId);
    }

    /// <summary>The item was confirmed lost or destroyed while on loan. This is a terminal state.</summary>
    public void MarkWrittenOff(IClock clock, Guid? staffId)
    {
        RequireStatus(EquipmentStatus.WrittenOff, EquipmentStatus.OnLoan);
        Status = EquipmentStatus.WrittenOff;
        Touch(clock, staffId);
    }

    private void RequireStatus(EquipmentStatus target, EquipmentStatus requiredCurrent)
    {
        if (Status != requiredCurrent)
        {
            throw new InvalidOperationException(
                $"Cannot change equipment status from {Status} to {target}; it must be {requiredCurrent}.");
        }
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Equipment name cannot be empty.", nameof(name));
        }

        string trimmed = name.Trim();

        if (trimmed.Length > NameMaxLength)
        {
            throw new ArgumentException($"Equipment name cannot exceed {NameMaxLength} characters.", nameof(name));
        }

        return trimmed;
    }

    private static string ValidateSerialNumber(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
        {
            throw new ArgumentException("Equipment serial number cannot be empty.", nameof(serialNumber));
        }

        string trimmed = serialNumber.Trim();

        if (trimmed.Length > SerialNumberMaxLength)
        {
            throw new ArgumentException(
                $"Equipment serial number cannot exceed {SerialNumberMaxLength} characters.",
                nameof(serialNumber));
        }

        return trimmed;
    }
}
