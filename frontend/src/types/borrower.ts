/// Shape returned by GET/POST/PUT /api/borrowers on the backend.
/// See docs/03-domenemodell.md for the Låntakerstatus state machine. Note
/// that `isUnreliable` is a flag, not a status - an unreliable borrower may
/// still borrow, a banned one may not. Keep the two separate in the UI too.
export type BorrowerStatus = 'Active' | 'Banned';

export type Borrower = {
  id: string;
  name: string;
  dateOfBirth: string;
  guardianId: string;
  guardianName: string;
  lateReturnCount: number;
  isUnreliable: boolean;
  status: BorrowerStatus;
  /// Archived borrowers are hidden from the working lists but restorable.
  /// Anonymised ones have had their identifying data stripped and cannot be
  /// restored - see ADR-0026 and docs/09-lover-og-regler.md.
  archivedAt: string | null;
  anonymisedAt: string | null;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};

/// Body for POST /api/borrowers. Exactly one of `guardianId` (link an
/// existing guardian) and `newGuardian` (create one in the same request)
/// must be given - enforced by the backend, not this type.
export type CreateBorrowerRequest = {
  name: string;
  dateOfBirth: string;
  guardianId?: string;
  newGuardian?: {
    name: string;
    email: string;
    phone: string;
    /// Optional - see docs/09-lover-og-regler.md. Omitting or sending false
    /// does not block registration.
    identityVerified?: boolean;
  };
};
