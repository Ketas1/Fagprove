'use client';

import Link from 'next/link';
import { useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';
import { RowActionsMenu } from '@/components/row-actions-menu';
import { StatusBadge } from '@/components/status-badge';
import { SearchInput } from '@/components/ui/search-input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { NewLoanDialog } from '@/components/loans/new-loan-dialog';
import { calculateAge } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import { loanStatusInfo } from '@/lib/status-labels';
import { normalizeSearchQuery } from '@/lib/search';
import type { Borrower } from '@/types/borrower';
import type { Equipment } from '@/types/equipment';
import type { Loan, LoanStatus } from '@/types/loan';

type StatusFilter = 'all' | LoanStatus;
type DateRangeFilter = 'today' | 'week' | 'month' | 'all';

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString('nb-NO');
}

/** Monday-start week, matching Norwegian calendar convention. */
function startOfWeek(date: Date): Date {
  const result = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  const day = result.getDay();
  const diffToMonday = day === 0 ? -6 : 1 - day;
  result.setDate(result.getDate() + diffToMonday);
  return result;
}

function matchesDateRange(startedAt: string, range: DateRangeFilter, now: Date): boolean {
  if (range === 'all') {
    return true;
  }

  const started = new Date(startedAt);

  if (range === 'today') {
    return started.toDateString() === now.toDateString();
  }

  if (range === 'week') {
    const start = startOfWeek(now);
    const end = new Date(start);
    end.setDate(end.getDate() + 7);
    return started >= start && started < end;
  }

  const start = new Date(now.getFullYear(), now.getMonth(), 1);
  const end = new Date(now.getFullYear(), now.getMonth() + 1, 1);
  return started >= start && started < end;
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
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>(initialStatusFilter);
  const [dateRangeFilter, setDateRangeFilter] = useState<DateRangeFilter>('all');
  // View toggle (Tabell/Kanban) is commented out, not removed - see the
  // commented-out LoansKanban render below and its definition at the bottom
  // of this file. Developer asked to keep the code for possible future use.
  // const [view, setView] = useState<'table' | 'kanban'>('table');

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
    const query = normalizeSearchQuery(search);
    const now = new Date();
    return withStatus.filter(({ loan, status }) => {
      const matchesStatus = statusFilter === 'all' || status === statusFilter;
      const matchesDate = matchesDateRange(loan.startedAt, dateRangeFilter, now);
      const matchesSearch =
        query.length === 0 ||
        loan.borrowerName.toLowerCase().includes(query) ||
        loan.equipmentName.toLowerCase().includes(query) ||
        loan.id.toLowerCase().includes(query);
      return matchesStatus && matchesDate && matchesSearch;
    });
  }, [withStatus, statusFilter, dateRangeFilter, search]);

  return (
    <div className="flex flex-col gap-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2.5">
          <SearchInput value={search} onChange={setSearch} placeholder="Søk låntaker eller utstyr…" />
          <Tabs value={statusFilter} onValueChange={(value) => setStatusFilter(value as StatusFilter)}>
            <TabsList>
              <TabsTrigger value="all">Alle ({counts.all})</TabsTrigger>
              <TabsTrigger value="Active">Aktiv ({counts.Active})</TabsTrigger>
              <TabsTrigger value="Overdue">Forfalt ({counts.Overdue})</TabsTrigger>
              <TabsTrigger value="Returned">Levert ({counts.Returned})</TabsTrigger>
              <TabsTrigger value="Lost">Tapt ({counts.Lost})</TabsTrigger>
            </TabsList>
          </Tabs>
          <Tabs value={dateRangeFilter} onValueChange={(value) => setDateRangeFilter(value as DateRangeFilter)}>
            <TabsList>
              <TabsTrigger value="today">I dag</TabsTrigger>
              <TabsTrigger value="week">Denne uken</TabsTrigger>
              <TabsTrigger value="month">Denne måneden</TabsTrigger>
              <TabsTrigger value="all">Alle</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <div className="flex items-center gap-2.5">
          {/*
            Table/Kanban view toggle - commented out, not removed, per
            developer instruction (kept for possible future use). Only the
            table view renders below now.
          */}
          {/* <Tabs value={view} onValueChange={(value) => setView(value as 'table' | 'kanban')}>
            <TabsList>
              <TabsTrigger value="table">Tabell</TabsTrigger>
              <TabsTrigger value="kanban">Kanban</TabsTrigger>
            </TabsList>
          </Tabs> */}
          <NewLoanDialog borrowers={borrowers} equipment={equipment} />
        </div>
      </div>

      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[20%]">Låntaker</TableHead>
            <TableHead className="w-[18%]">Utstyr</TableHead>
            <TableHead className="w-[12%]">Startdato</TableHead>
            <TableHead className="w-[12%]">Forventet retur</TableHead>
            <TableHead className="w-[13%]">Status</TableHead>
            <TableHead className="w-[17%]">Kontaktstatus</TableHead>
            <TableHead className="w-[8%]">
              <span className="sr-only">Handlinger</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filtered.length === 0 && (
            <TableRow>
              <TableCell colSpan={7} className="py-8 text-center text-sm text-muted-foreground">
                Ingen utlån matcher søket.
              </TableCell>
            </TableRow>
          )}
          {filtered.map(({ loan, status }) => {
            const borrower = borrowersById.get(loan.borrowerId);
            const { label, tone } = loanStatusInfo(status);
            const detailHref = `/dashboard/loans/${loan.id}`;

            return (
              <TableRow
                key={loan.id}
                onClick={() => router.push(detailHref)}
                className="cursor-pointer hover:bg-muted/40"
              >
                <TableCell className="max-w-0" onClick={(event) => event.stopPropagation()}>
                  <Link href={detailHref} className="block truncate font-medium hover:underline">
                    {loan.borrowerName}
                  </Link>
                  {borrower && (
                    <div className="text-xs text-muted-foreground">
                      {calculateAge(borrower.dateOfBirth, new Date())} år
                    </div>
                  )}
                </TableCell>
                <TableCell className="max-w-0 truncate">{loan.equipmentName}</TableCell>
                <TableCell>{formatDate(loan.startedAt)}</TableCell>
                <TableCell className={status === 'Overdue' ? 'font-medium text-status-danger-fg' : undefined}>
                  {formatDate(loan.dueDate)}
                </TableCell>
                <TableCell>
                  <StatusBadge tone={tone}>{label}</StatusBadge>
                </TableCell>
                <TableCell className="text-[12.5px] text-muted-foreground">Se detaljer</TableCell>
                <TableCell onClick={(event) => event.stopPropagation()}>
                  <RowActionsMenu openHref={detailHref} label={`Handlinger for ${loan.borrowerName}`} />
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
      {/* Kanban view render - commented out alongside the toggle above. */}
      {/* {view === 'kanban' && <LoansKanban loans={filtered} />} */}
    </div>
  );
}

// LoansKanban - commented out, not deleted, per developer instruction (kept
// for possible future use). Its only caller (the view toggle above) is
// commented out too, so this would otherwise be dead code and fail lint.
//
// function LoansKanban({ loans }: { loans: { loan: Loan; status: LoanStatus }[] }) {
//   const columns: LoanStatus[] = ['Active', 'Overdue', 'Returned', 'Lost'];
//
//   return (
//     <div className="grid grid-cols-4 gap-4">
//       {columns.map((column) => {
//         const { label, tone } = loanStatusInfo(column);
//         const items = loans.filter((item) => item.status === column);
//
//         return (
//           <div key={column} className="flex flex-col gap-2.5">
//             <div className="flex items-center gap-2 text-sm font-medium">
//               {label} <span className="text-xs text-muted-foreground">{items.length}</span>
//             </div>
//             <div className="flex flex-col gap-2">
//               {items.map(({ loan }) => (
//                 <Link
//                   key={loan.id}
//                   href={`/dashboard/loans/${loan.id}`}
//                   className="flex flex-col gap-1 rounded-lg border p-3 text-sm hover:bg-muted/40"
//                 >
//                   <span className="font-medium">{loan.borrowerName}</span>
//                   <span className="text-muted-foreground">{loan.equipmentName}</span>
//                   <StatusBadge tone={tone} className="w-fit">
//                     {formatDate(loan.dueDate)}
//                   </StatusBadge>
//                 </Link>
//               ))}
//               {items.length === 0 && (
//                 <div className="rounded-lg border border-dashed p-3 text-center text-xs text-muted-foreground">
//                   Ingen
//                 </div>
//               )}
//             </div>
//           </div>
//         );
//       })}
//     </div>
//   );
// }
