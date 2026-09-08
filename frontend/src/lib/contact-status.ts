import type { StatusTone } from '@/components/status-badge';
import type { Loan } from '@/types/loan';

/**
 * Norwegian label and badge tone for how far follow-up on a loan has come.
 * Business rule 5 in docs/03-domenemodell.md says staff must be able to see
 * whether a guardian has already been contacted - this puts that on the loan
 * lists, so it is visible without opening each loan.
 *
 * `lastContactedAt` is null exactly when the count is zero (the backend
 * derives both from the same rows), so the two fields cannot disagree; the
 * null check is a type guard, not a second case.
 */
export function contactStatusInfo(
  loan: Pick<Loan, 'contactAttemptCount' | 'lastContactedAt'>,
): { label: string; tone: StatusTone } {
  if (loan.contactAttemptCount === 0 || loan.lastContactedAt === null) {
    return { label: 'Ikke kontaktet', tone: 'warning' };
  }

  // "forsøk" is a neuter noun - the plural is identical, so no branch here.
  return {
    label: `${loan.contactAttemptCount} forsøk · ${formatDayMonth(loan.lastContactedAt)}`,
    tone: 'success',
  };
}

/**
 * "dd.MM" built from the parts rather than `toLocaleDateString`, which
 * depends on the runtime's ICU data and would make this untestable across
 * environments.
 */
function formatDayMonth(value: string): string {
  const date = new Date(value);
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  return `${day}.${month}`;
}
