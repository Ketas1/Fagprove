import type { Loan, LoanStatus } from '@/types/loan';

/**
 * The backend's background job (`OverdueLoanBackgroundService`) materializes
 * Active -> Overdue on a timer, not instantly - a loan whose due date has
 * just passed can still report `status: "Active"` from the API for up to
 * one tick. This recomputes the same read-time check the backend's own
 * `Loan.IsOverdueNow` uses (see docs/adr/0011-automatisk-forfall.md) so the
 * dashboard and loan list are correct immediately, not just eventually.
 *
 * Compares calendar dates (UTC), not exact instants - matches the backend
 * fix in Loan.cs: a loan due "7/9" is not overdue until "8/9" begins,
 * regardless of what time-of-day `dueDate` carries.
 *
 * `now` is a parameter, not `new Date()` inline, so overdue logic stays
 * testable without touching the system clock - see CLAUDE.md.
 */
export function effectiveLoanStatus(loan: Pick<Loan, 'status' | 'dueDate'>, now: Date): LoanStatus {
  if (loan.status === 'Active' && isPastDueDate(loan.dueDate, now)) {
    return 'Overdue';
  }

  return loan.status;
}

function isPastDueDate(dueDate: string, now: Date): boolean {
  const due = new Date(dueDate);
  const nowUtcDate = Date.UTC(now.getUTCFullYear(), now.getUTCMonth(), now.getUTCDate());
  const dueUtcDate = Date.UTC(due.getUTCFullYear(), due.getUTCMonth(), due.getUTCDate());
  return nowUtcDate > dueUtcDate;
}
