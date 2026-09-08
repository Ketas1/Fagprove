/// Shape returned by GET/POST/PUT /api/guardians on the backend.
export type Guardian = {
  id: string;
  name: string;
  email: string;
  phone: string;
  /// Set when staff have visually confirmed this guardian's ID in person -
  /// this project's alternative to storing a fødselsnummer, see
  /// docs/09-lover-og-regler.md. Null means not confirmed yet; that never
  /// blocks anything.
  identityVerifiedAt: string | null;
  /// Same archive/anonymise lifecycle as Borrower - archived is reversible,
  /// anonymised is not. See ADR-0026.
  archivedAt: string | null;
  anonymisedAt: string | null;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};
