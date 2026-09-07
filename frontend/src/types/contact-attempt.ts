/// Shape returned by GET/POST /api/loans/{id}/contact-attempts on the backend.
export type ContactMethod = 'Email' | 'Phone';

export type ContactAttempt = {
  id: string;
  loanId: string;
  method: ContactMethod;
  outcome: string;
  createdAt: string;
  createdByStaffId: string | null;
};
