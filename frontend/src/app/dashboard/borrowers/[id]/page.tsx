import Link from 'next/link';
import { notFound } from 'next/navigation';
import { AlertTriangle, ArrowLeft } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { BorrowerBanSection } from '@/components/borrowers/borrower-ban-section';
import { BorrowerNotesSection } from '@/components/borrowers/borrower-notes-section';
import { StatusBadge } from '@/components/status-badge';
import { BackendError, fetchBackend } from '@/lib/backend';
import { calculateAge } from '@/lib/age';
import { borrowerStatusInfo } from '@/lib/status-labels';
import type { Ban } from '@/types/ban';
import type { Borrower } from '@/types/borrower';
import type { Guardian } from '@/types/guardian';
import type { Note } from '@/types/note';

export const dynamic = 'force-dynamic';

export default async function BorrowerDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  let borrower: Borrower;

  try {
    borrower = await fetchBackend<Borrower>(['borrowers', id]);
  } catch (error) {
    if (error instanceof BackendError && error.status === 404) {
      notFound();
    }
    throw error;
  }

  // Borrower only carries `guardianName` - contact details live on the
  // Guardian record itself, fetched separately. The current ban is only
  // fetched when actually banned - GET .../ban 404s otherwise, since there
  // is no "current ban" resource for an active borrower.
  const [guardian, notes, currentBan] = await Promise.all([
    fetchBackend<Guardian>(['guardians', borrower.guardianId]),
    fetchBackend<Note[]>(['borrowers', id, 'notes']),
    borrower.status === 'Banned' ? fetchBackend<Ban>(['borrowers', id, 'ban']) : Promise.resolve(null),
  ]);

  const { label: statusLabel, tone: statusTone } = borrowerStatusInfo(borrower.status);
  const age = calculateAge(borrower.dateOfBirth, new Date());

  return (
    <div className="flex flex-col gap-4">
      <Link
        href="/dashboard/borrowers"
        className="flex w-fit items-center gap-1.5 text-[12.5px] font-medium text-muted-foreground"
      >
        <ArrowLeft className="size-3.5" /> Tilbake til barn og foresatte
      </Link>

      <div className="flex items-center gap-2.5">
        <h1 className="text-xl font-semibold tracking-tight">{borrower.name}</h1>
        <StatusBadge tone={statusTone} className="h-[26px] px-3 text-[12.5px]">
          {statusLabel}
        </StatusBadge>
      </div>
      <p className="-mt-2 text-[12.5px] text-muted-foreground">{age} år</p>

      {borrower.isUnreliable && (
        <div className="flex w-fit items-center gap-2 rounded-lg bg-status-warning-bg px-3 py-2 text-[12.5px] text-status-warning-fg">
          <AlertTriangle className="size-3.5 shrink-0" />
          Markert som upålitelig - har levert for sent {borrower.lateReturnCount}{' '}
          {borrower.lateReturnCount === 1 ? 'gang' : 'ganger'} tidligere.
        </div>
      )}

      <div className="grid gap-4 sm:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>Detaljer</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-x-5 gap-y-4">
            <Field label="Fødselsdato" value={new Date(borrower.dateOfBirth).toLocaleDateString('nb-NO')} />
            <Field label="Sene returer" value={String(borrower.lateReturnCount)} />
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>Foresatt</CardTitle>
          </CardHeader>
          <CardContent className="grid grid-cols-2 gap-x-5 gap-y-4">
            <Field label="Navn" value={guardian.name} />
            <Field label="Telefon" value={guardian.phone} />
            <Field label="E-post" value={guardian.email} />
          </CardContent>
        </Card>
      </div>

      <div className="grid gap-4 sm:grid-cols-2">
        <BorrowerBanSection borrowerId={id} status={borrower.status} initialBan={currentBan} />
        <BorrowerNotesSection borrowerId={id} initialNotes={notes} />
      </div>
    </div>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="text-[11.5px] font-medium text-muted-foreground">{label}</div>
      <div className="mt-1 text-sm font-medium">{value}</div>
    </div>
  );
}
