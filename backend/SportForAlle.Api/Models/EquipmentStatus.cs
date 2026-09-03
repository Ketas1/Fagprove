namespace SportForAlle.Api.Models;

/// <summary>See the "Utstyrstatus" state machine in docs/03-domenemodell.md.</summary>
public enum EquipmentStatus
{
    Available,
    OnLoan,
    OutOfService,
    WrittenOff,
}
