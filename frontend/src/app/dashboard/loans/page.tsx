import { LoansExplorer } from '@/components/loans/loans-explorer';
import { fetchBackend } from '@/lib/backend';
import type { Borrower } from '@/types/borrower';
import type { Equipment } from '@/types/equipment';
import type { Loan, LoanStatus } from '@/types/loan';

export const dynamic = 'force-dynamic';

const KNOWN_STATUSES: LoanStatus[] = ['Active', 'Overdue', 'Returned', 'Lost'];

export default async function LoansPage({
  searchParams,
}: {
  searchParams: Promise<{ status?: string }>;
}) {
  const { status } = await searchParams;
  const initialStatusFilter =
    status && KNOWN_STATUSES.includes(status as LoanStatus) ? (status as LoanStatus) : 'all';

  const [loans, borrowers, equipment] = await Promise.all([
    fetchBackend<Loan[]>(['loans']),
    fetchBackend<Borrower[]>(['borrowers']),
    fetchBackend<Equipment[]>(['equipment']),
  ]);

  return (
    <LoansExplorer
      loans={loans}
      borrowers={borrowers}
      equipment={equipment}
      initialStatusFilter={initialStatusFilter}
    />
  );
}
