'use client';

import { useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';
import { AlertTriangle } from 'lucide-react';
import { RowActionsMenu } from '@/components/row-actions-menu';
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

type Filter = 'all' | 'active' | 'banned' | 'unreliable';

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

  const counts = useMemo(
    () => ({
      all: borrowers.length,
      active: borrowers.filter((b) => b.status === 'Active').length,
      banned: borrowers.filter((b) => b.status === 'Banned').length,
      unreliable: borrowers.filter((b) => b.isUnreliable).length,
    }),
    [borrowers],
  );

  const filtered = useMemo(() => {
    const query = normalizeSearchQuery(search);
    return borrowers.filter((borrower) => {
      const matchesFilter =
        filter === 'all' ||
        (filter === 'active' && borrower.status === 'Active') ||
        (filter === 'banned' && borrower.status === 'Banned') ||
        (filter === 'unreliable' && borrower.isUnreliable);
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
                    onEdit={() => setEditingBorrower(borrower)}
                    label={`Handlinger for ${borrower.name}`}
                  />
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>

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
