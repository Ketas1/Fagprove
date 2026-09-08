using SportForAlle.Api.Validation;

namespace SportForAlle.Api.Services.Rules;

public static class EquipmentRules
{
    /// <summary>
    /// SerialNumber is uniquely indexed (see EquipmentConfiguration). Checking
    /// it here turns what would be a raw constraint violation into a clean
    /// 409, on update as well as create.
    /// </summary>
    public static void EnsureSerialNumberIsUnique(bool duplicateExists)
    {
        if (duplicateExists)
        {
            throw new DomainConflictException(
                "DuplicateSerialNumber", "Serienummeret er allerede i bruk.");
        }
    }

    /// <summary>
    /// Equipment with loan history must not be deleted - the history is the
    /// basis for reporting to the municipality, see docs/04-databasedesign.md,
    /// "Nøkler og relasjoner". The database enforces this too (`Restrict` on
    /// `Loan.EquipmentId`); checking here first turns that into a clean
    /// `409` instead of a raw constraint error.
    /// </summary>
    public static void EnsureCanBeDeleted(bool hasLoanHistory)
    {
        if (hasLoanHistory)
        {
            throw new DomainConflictException(
                "EquipmentHasLoanHistory", "Utstyret har lånehistorikk og kan ikke slettes.");
        }
    }
}
