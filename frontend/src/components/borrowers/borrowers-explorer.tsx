'use client';

import { useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { ConfirmDialog } from '@/components/confirm-dialog';
import { Archive, ArchiveRestore, Trash2, UserX } from 'lucide-react';
import { RowActionsMenu, type RowAction } from '@/components/row-actions-menu';
import { StatusBadge } from '@/components/status-badge';
import { SearchInput } from '@/components/ui/search-input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { EditBorrowerDialog } from '@/components/borrowers/edit-borrower-dialog';
import { RegisterChildDialog } from '@/components/borrowers/register-child-dialog';
import { calculateAge } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import { borrowerStatusInfo } from '@/lib/status-labels';
import { normalizeSearchQuery } from '@/lib/search';
import type { Borrower } from '@/types/borrower';
import type { Guardian } from '@/types/guardian';
import type { Loan } from '@/types/loan';

type Filter = 'all' | 'active' | 'banned' | 'unreliable' | 'archived';

export function BorrowersExplorer({
  borrowers,
  loans,
  guardians,
}: {
  borrowers: Borrower[];
  loans: Loan[];
  guardians: Guardian[];
}) {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<Filter>('all');
  const [editingBorrower, setEditingBorrower] = useState<Borrower | null>(null);
  const [lifecycleTarget, setLifecycleTarget] = useState<{ borrower: Borrower; kind: LifecycleKind } | null>(null);

  const activeLoanCountByBorrower = useMemo(() => {
    const now = new Date();
    const counts = new Map<string, number>();
    for (const loan of loans) {
      const status = effectiveLoanStatus(loan, now);
      if (status === 'Active' || status === 'Overdue') {
        counts.set(loan.borrowerId, (counts.get(loan.borrowerId) ?? 0) + 1);
      }
    }
    return counts;
  }, [loans]);

  // Every count except "Arkivert" excludes archived borrowers, so each tab
  // label matches the number of rows it actually shows.
  const counts = useMemo(() => {
    const live = borrowers.filter((b) => b.archivedAt === null);
    return {
      all: live.length,
      active: live.filter((b) => b.status === 'Active').length,
      banned: live.filter((b) => b.status === 'Banned').length,
      unreliable: live.filter((b) => b.isUnreliable).length,
      archived: borrowers.length - live.length,
    };
  }, [borrowers]);

  const filtered = useMemo(() => {
    const query = normalizeSearchQuery(search);
    return borrowers.filter((borrower) => {
      // Archived borrowers are hidden from every working view - that is what
      // archiving is for - and reachable only through their own tab.
      const isArchived = borrower.archivedAt !== null;
      const matchesFilter =
        filter === 'archived'
          ? isArchived
          : !isArchived &&
            (filter === 'all' ||
              (filter === 'active' && borrower.status === 'Active') ||
              (filter === 'banned' && borrower.status === 'Banned') ||
              (filter === 'unreliable' && borrower.isUnreliable));
      const matchesSearch =
        query.length === 0 ||
        borrower.name.toLowerCase().includes(query) ||
        borrower.guardianName.toLowerCase().includes(query);
      return matchesFilter && matchesSearch;
    });
  }, [borrowers, filter, search]);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2.5">
          <SearchInput value={search} onChange={setSearch} placeholder="Søk barn eller foresatt…" />
          <Tabs value={filter} onValueChange={(value) => setFilter(value as Filter)}>
            <TabsList>
              <TabsTrigger value="all">Alle ({counts.all})</TabsTrigger>
              <TabsTrigger value="active">Aktive ({counts.active})</TabsTrigger>
              <TabsTrigger value="banned">Utestengt ({counts.banned})</TabsTrigger>
              <TabsTrigger value="unreliable">Upålitelig ({counts.unreliable})</TabsTrigger>
              <TabsTrigger value="archived">Arkivert ({counts.archived})</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <RegisterChildDialog guardians={guardians} />
      </div>

      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[28%]">Barn</TableHead>
            <TableHead className="w-[24%]">Foresatt</TableHead>
            <TableHead className="w-[14%]">Status</TableHead>
            <TableHead className="w-[13%]">Aktive lån</TableHead>
            <TableHead className="w-[13%]">Sene returer</TableHead>
            <TableHead className="w-[8%]">
              <span className="sr-only">Handlinger</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filtered.length === 0 && (
            <TableRow>
              <TableCell colSpan={6} className="py-8 text-center text-sm text-muted-foreground">
                Ingen barn matcher søket.
              </TableCell>
            </TableRow>
          )}
          {filtered.map((borrower) => {
            const age = calculateAge(borrower.dateOfBirth, new Date());
            const { label, tone } = borrowerStatusInfo(borrower.status);
            const detailHref = `/dashboard/borrowers/${borrower.id}`;

            return (
              <TableRow
                key={borrower.id}
                onClick={() => router.push(detailHref)}
                className="cursor-pointer hover:bg-muted/40"
              >
                <TableCell className="max-w-0">
                  <div className="flex items-center gap-1.5 truncate font-medium">
                    <span className="truncate">{borrower.name}</span>
                    {borrower.isUnreliable && (
                      <AlertTriangle className="size-3.5 shrink-0 text-status-warning-fg" />
                    )}
                  </div>
                  <div className="text-xs text-muted-foreground">{age} år</div>
                </TableCell>
                <TableCell className="max-w-0 truncate">{borrower.guardianName}</TableCell>
                <TableCell>
                  <StatusBadge tone={tone}>{label}</StatusBadge>
                </TableCell>
                <TableCell>{activeLoanCountByBorrower.get(borrower.id) ?? 0}</TableCell>
                <TableCell>{borrower.lateReturnCount}</TableCell>
                <TableCell onClick={(event) => event.stopPropagation()}>
                  <RowActionsMenu
                    openHref={detailHref}
                    onEdit={borrower.anonymisedAt ? undefined : () => setEditingBorrower(borrower)}
                    actions={lifecycleActions(borrower, (kind) => setLifecycleTarget({ borrower, kind }))}
                    label={`Handlinger for ${borrower.name}`}
                  />
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>

      {lifecycleTarget && (
        <ConfirmDialog
          open
          onOpenChange={(open) => !open && setLifecycleTarget(null)}
          title={LIFECYCLE_COPY[lifecycleTarget.kind].title}
          description={LIFECYCLE_COPY[lifecycleTarget.kind].description(lifecycleTarget.borrower.name)}
          confirmLabel={LIFECYCLE_COPY[lifecycleTarget.kind].confirmLabel}
          destructive={LIFECYCLE_COPY[lifecycleTarget.kind].destructive}
          action={() => runLifecycle(lifecycleTarget.borrower.id, lifecycleTarget.kind)}
        />
      )}

      {editingBorrower && (
        <EditBorrowerDialog
          borrower={editingBorrower}
          open
          onOpenChange={(open) => !open && setEditingBorrower(null)}
        />
      )}
    </div>
  );
}

/** Which lifecycle action the confirmation dialog is currently asking about. */
type LifecycleKind = 'archive' | 'restore' | 'anonymise' | 'delete';

/**
 * Archiving is reversible, anonymising and deleting are not. Delete is only
 * offered for a borrower with no history at all - the backend refuses the
 * rest with 409 BorrowerHasHistory, and offering a button that always fails
 * would be worse than not offering it. See ADR-0026.
 */
function lifecycleActions(borrower: Borrower, ask: (kind: LifecycleKind) => void): RowAction[] {
  if (borrower.anonymisedAt) {
    return [];
  }

  const actions: RowAction[] = borrower.archivedAt
    ? [{ label: 'Gjenopprett', icon: ArchiveRestore, onClick: () => ask('restore') }]
    : [{ label: 'Arkiver', icon: Archive, onClick: () => ask('archive') }];

  actions.push({ label: 'Anonymiser', icon: UserX, onClick: () => ask('anonymise'), destructive: true });
  actions.push({ label: 'Slett', icon: Trash2, onClick: () => ask('delete'), destructive: true });

  return actions;
}

const LIFECYCLE_COPY: Record<
  LifecycleKind,
  { title: string; confirmLabel: string; destructive: boolean; description: (name: string) => string }
> = {
  archive: {
    title: 'Arkiver låntaker',
    confirmLabel: 'Arkiver',
    destructive: false,
    description: (name) =>
      `${name} skjules fra listene, men ingenting slettes. Du kan gjenopprette når som helst.`,
  },
  restore: {
    title: 'Gjenopprett låntaker',
    confirmLabel: 'Gjenopprett',
    destructive: false,
    description: (name) => `${name} blir synlig i listene igjen.`,
  },
  anonymise: {
    title: 'Anonymiser låntaker',
    confirmLabel: 'Anonymiser',
    destructive: true,
    description: (name) =>
      `Navnet på ${name} fjernes permanent, og notatene slettes. Utlånene beholdes uten navn, slik at ` +
      'rapportene til kommunen fortsatt stemmer. Dette kan ikke angres.',
  },
  delete: {
    title: 'Slett låntaker',
    confirmLabel: 'Slett',
    destructive: true,
    description: (name) =>
      `${name} slettes permanent. Dette er bare mulig når låntakeren ikke har utlån, utestengelser ` +
      'eller notater - har hen det, bruk Anonymiser i stedet.',
  },
};

function runLifecycle(borrowerId: string, kind: LifecycleKind): Promise<Response> {
  switch (kind) {
    case 'archive':
      return fetch(`/api/borrowers/${borrowerId}/archive`, { method: 'POST' });
    case 'restore':
      return fetch(`/api/borrowers/${borrowerId}/archive`, { method: 'DELETE' });
    case 'anonymise':
      return fetch(`/api/borrowers/${borrowerId}/anonymise`, { method: 'POST' });
    case 'delete':
      return fetch(`/api/borrowers/${borrowerId}`, { method: 'DELETE' });
  }
}
