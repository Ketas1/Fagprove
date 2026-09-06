import type { EquipmentCondition } from './equipment';

/// Shape returned by GET/POST /api/loans and POST /api/loans/{id}/return on the backend.
/// See docs/03-domenemodell.md for the LoanStatus state machine.
export type LoanStatus = 'Active' | 'Overdue' | 'Returned' | 'Lost';

export type Loan = {
  id: string;
  borrowerId: string;
  borrowerName: string;
  equipmentId: string;
  equipmentName: string;
  startedAt: string;
  dueDate: string;
  returnedAt: string | null;
  daysLate: number | null;
  status: LoanStatus;
  createdByStaffId: string | null;
  updatedByStaffId: string | null;
};

/// Body for POST /api/loans.
export type CreateLoanRequest = {
  borrowerId: string;
  equipmentId: string;
  dueDate: string;
};

/// Body for POST /api/loans/{id}/return.
export type ReturnLoanRequest = {
  condition: EquipmentCondition;
};
