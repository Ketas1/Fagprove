using SportForAlle.Api.Models;
using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services.Rules;

public static class StaffRules
{
    /// <summary>
    /// A Staff profile can only ever be linked to one Auth0 account -
    /// re-linking an already-linked profile would silently hand access to
    /// whoever calls link-me next.
    /// </summary>
    /// <summary>
    /// An employee must not delete the profile their own Auth0 account is
    /// linked to - that would lock them straight out of the system, since
    /// RequireLinkedStaffMiddleware rejects every request from an unlinked
    /// account.
    /// </summary>
    public static void EnsureNotDeletingOwnProfile(Staff staff, string? currentAuth0Subject)
    {
        if (currentAuth0Subject is not null && staff.Auth0UserId == currentAuth0Subject)
        {
            throw new DomainConflictException(
                "CannotDeleteOwnStaffProfile", "Du kan ikke slette din egen ansattprofil.");
        }
    }

    public static void EnsureNotAlreadyLinked(Staff staff)
    {
        if (staff.Auth0UserId is not null)
        {
            throw new DomainConflictException(
                "StaffAlreadyLinked", "Denne ansattprofilen er allerede koblet til en Auth0-konto.");
        }
    }

    /// <summary>
    /// One Auth0 account can only ever be linked to one Staff profile -
    /// enforced here for a clean, reasoned reject, and by the database's
    /// unique index on <see cref="Staff.Auth0UserId"/> as a backstop.
    /// </summary>
    public static void EnsureAuth0AccountNotLinkedElsewhere(bool isLinkedToAnotherStaff)
    {
        if (isLinkedToAnotherStaff)
        {
            throw new DomainConflictException(
                "Auth0AccountAlreadyLinked", "Denne Auth0-kontoen er allerede koblet til en annen ansattprofil.");
        }
    }
}
