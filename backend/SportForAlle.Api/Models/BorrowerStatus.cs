namespace SportForAlle.Api.Models;

/// <summary>
/// See the "Låntakerstatus" state machine in docs/03-domenemodell.md.
/// </summary>
/// <remarks>
/// This is distinct from <see cref="Borrower.IsUnreliable"/>, which is a
/// warning flag, not a status: an unreliable borrower may still borrow, a
/// banned one may not. The two must never be merged.
/// </remarks>
public enum BorrowerStatus
{
    Active,
    Banned,
}
