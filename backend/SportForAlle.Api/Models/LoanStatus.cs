namespace SportForAlle.Api.Models;

/// <summary>See the "Lånestatus" state machine in docs/03-domenemodell.md.</summary>
public enum LoanStatus
{
    Active,
    Overdue,
    Returned,
    Lost,
}
