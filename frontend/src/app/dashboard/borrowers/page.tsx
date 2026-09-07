import { BorrowersExplorer } from '@/components/borrowers/borrowers-explorer';
import { fetchBackend } from '@/lib/backend';
import type { Borrower } from '@/types/borrower';
import type { Guardian } from '@/types/guardian';
import type { Loan } from '@/types/loan';

export const dynamic = 'force-dynamic';

export default async function BorrowersPage() {
  const [borrowers, loans, guardians] = await Promise.all([
    fetchBackend<Borrower[]>(['borrowers']),
    fetchBackend<Loan[]>(['loans']),
    fetchBackend<Guardian[]>(['guardians']),
  ]);

  return <BorrowersExplorer borrowers={borrowers} loans={loans} guardians={guardians} />;
}
