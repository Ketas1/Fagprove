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
  };
};
