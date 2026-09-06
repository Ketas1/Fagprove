import type { Loan, LoanStatus } from '@/types/loan';

/**
 * The backend's background job that materializes Active -> Overdue on the
 * stored row hasn't been built yet (see docs/adr/0011-automatisk-forfall.md
 * - only the read-time half, `Loan.IsOverdueNow`, exists). A loan whose due
 * date has passed can therefore still report `status: "Active"` from the
 * API. This recomputes the same read-time check on the frontend so the
 * dashboard and loan list never depend on that job existing.
 *
 * `now` is a parameter, not `new Date()` inline, so overdue logic stays
 * testable without touching the system clock - see CLAUDE.md.
 */
export function effectiveLoanStatus(loan: Pick<Loan, 'status' | 'dueDate'>, now: Date): LoanStatus {
  if (loan.status === 'Active' && new Date(loan.dueDate) < now) {
    return 'Overdue';
  }

  return loan.status;
}
