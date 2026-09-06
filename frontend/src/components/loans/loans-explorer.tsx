'use client';

import Link from 'next/link';
import { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { NotBuiltYetBadge } from '@/components/not-built-yet';
import { StatusBadge } from '@/components/status-badge';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { NewLoanDialog } from '@/components/loans/new-loan-dialog';
import { calculateAge, ageGroup } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import { loanStatusInfo } from '@/lib/status-labels';
import type { Borrower } from '@/types/borrower';
import type { Equipment } from '@/types/equipment';
import type { Loan, LoanStatus } from '@/types/loan';

type StatusFilter = 'all' | LoanStatus;

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString('nb-NO');
}

export function LoansExplorer({
  loans,
  borrowers,
  equipment,
  initialStatusFilter,
}: {
  loans: Loan[];
  borrowers: Borrower[];
  equipment: Equipment[];
  initialStatusFilter: StatusFilter;
}) {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(initialStatusFilter);
  const [view, setView] = useState<'table' | 'kanban'>('table');

  const borrowersById = useMemo(() => new Map(borrowers.map((b) => [b.id, b])), [borrowers]);

  const withStatus = useMemo(() => {
    const now = new Date();
    return loans.map((loan) => ({ loan, status: effectiveLoanStatus(loan, now) }));
  }, [loans]);

  const counts = useMemo(() => {
    const result: Record<StatusFilter, number> = { all: withStatus.length, Active: 0, Overdue: 0, Returned: 0, Lost: 0 };
    for (const { status } of withStatus) {
      result[status] += 1;
    }
    return result;
  }, [withStatus]);

  const filtered = useMemo(() => {
    const query = search.trim().toLowerCase();
    return withStatus.filter(({ loan, status }) => {
      const matchesStatus = statusFilter === 'all' || status === statusFilter;
      const matchesSearch =
        query.length === 0 ||
        loan.borrowerName.toLowerCase().includes(query) ||
        loan.equipmentName.toLowerCase().includes(query);
      return matchesStatus && matchesSearch;
    });
  }, [withStatus, statusFilter, search]);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2.5">
          <div className="relative">
            <Search className="pointer-events-none absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Søk låntaker eller utstyr…"
              className="w-60 pl-8"
            />
          </div>
          <Tabs value={statusFilter} onValueChange={(value) => setStatusFilter(value as StatusFilter)}>
            <TabsList>
              <TabsTrigger value="all">Alle ({counts.all})</TabsTrigger>
              <TabsTrigger value="Active">Aktiv ({counts.Active})</TabsTrigger>
              <TabsTrigger value="Overdue">Forfalt ({counts.Overdue})</TabsTrigger>
              <TabsTrigger value="Returned">Levert ({counts.Returned})</TabsTrigger>
              <TabsTrigger value="Lost">Tapt ({counts.Lost})</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <div className="flex items-center gap-2.5">
          <Tabs value={view} onValueChange={(value) => setView(value as 'table' | 'kanban')}>
            <TabsList>
              <TabsTrigger value="table">Tabell</TabsTrigger>
              <TabsTrigger value="kanban">Kanban</TabsTrigger>
            </TabsList>
          </Tabs>
          <NewLoanDialog borrowers={borrowers} equipment={equipment} />
        </div>
      </div>

      {view === 'table' ? (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Låntaker</TableHead>
              <TableHead>Utstyr</TableHead>
              <TableHead>Startdato</TableHead>
              <TableHead>Forventet retur</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>
                <div className="flex items-center gap-1.5">
                  Kontaktstatus <NotBuiltYetBadge reason="Kontaktforsøk-logging finnes ikke i API-et ennå." />
                </div>
              </TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {filtered.length === 0 && (
              <TableRow>
                <TableCell colSpan={6} className="py-8 text-center text-sm text-muted-foreground">
                  Ingen utlån matcher søket.
                </TableCell>
              </TableRow>
            )}
            {filtered.map(({ loan, status }) => {
              const borrower = borrowersById.get(loan.borrowerId);
              const { label, tone } = loanStatusInfo(status);

              return (
                <TableRow key={loan.id}>
                  <TableCell>
                    <Link href={`/dashboard/loans/${loan.id}`} className="font-medium hover:underline">
                      {loan.borrowerName}
                    </Link>
                    {borrower && (
                      <div className="text-xs text-muted-foreground">
                        {calculateAge(borrower.dateOfBirth, new Date())} år ·{' '}
                        {ageGroup(calculateAge(borrower.dateOfBirth, new Date())) ?? '–'}
                      </div>
                    )}
                  </TableCell>
                  <TableCell>{loan.equipmentName}</TableCell>
                  <TableCell>{formatDate(loan.startedAt)}</TableCell>
                  <TableCell className={status === 'Overdue' ? 'font-medium text-status-danger-fg' : undefined}>
                    {formatDate(loan.dueDate)}
                  </TableCell>
                  <TableCell>
                    <StatusBadge tone={tone}>{label}</StatusBadge>
                  </TableCell>
                  <TableCell className="text-muted-foreground">–</TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      ) : (
        <LoansKanban loans={filtered} />
      )}
    </div>
  );
}

function LoansKanban({ loans }: { loans: { loan: Loan; status: LoanStatus }[] }) {
  const columns: LoanStatus[] = ['Active', 'Overdue', 'Returned', 'Lost'];

  return (
    <div className="grid grid-cols-4 gap-4">
      {columns.map((column) => {
        const { label, tone } = loanStatusInfo(column);
        const items = loans.filter((item) => item.status === column);

        return (
          <div key={column} className="flex flex-col gap-2.5">
            <div className="flex items-center gap-2 text-sm font-medium">
              {label} <span className="text-xs text-muted-foreground">{items.length}</span>
            </div>
            <div className="flex flex-col gap-2">
              {items.map(({ loan }) => (
                <Link
                  key={loan.id}
                  href={`/dashboard/loans/${loan.id}`}
                  className="flex flex-col gap-1 rounded-lg border p-3 text-sm hover:bg-muted/40"
                >
                  <span className="font-medium">{loan.borrowerName}</span>
                  <span className="text-muted-foreground">{loan.equipmentName}</span>
                  <StatusBadge tone={tone} className="w-fit">
                    {formatDate(loan.dueDate)}
                  </StatusBadge>
                </Link>
              ))}
              {items.length === 0 && (
                <div className="rounded-lg border border-dashed p-3 text-center text-xs text-muted-foreground">
                  Ingen
                </div>
              )}
            </div>
          </div>
        );
      })}
    </div>
  );
}
