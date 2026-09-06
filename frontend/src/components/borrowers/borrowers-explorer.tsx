'use client';

import { useMemo, useState } from 'react';
import { AlertTriangle, Search } from 'lucide-react';
import { StatusBadge } from '@/components/status-badge';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { RegisterChildDialog } from '@/components/borrowers/register-child-dialog';
import { calculateAge, ageGroup } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import { borrowerStatusInfo } from '@/lib/status-labels';
import type { Borrower } from '@/types/borrower';
import type { Loan } from '@/types/loan';

type Filter = 'all' | 'active' | 'banned' | 'unreliable';

export function BorrowersExplorer({ borrowers, loans }: { borrowers: Borrower[]; loans: Loan[] }) {
  const [search, setSearch] = useState('');
  const [filter, setFilter] = useState<Filter>('all');

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
    const query = search.trim().toLowerCase();
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
          <div className="relative">
            <Search className="pointer-events-none absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Søk barn eller foresatt…"
              className="w-64 pl-8"
            />
          </div>
          <Tabs value={filter} onValueChange={(value) => setFilter(value as Filter)}>
            <TabsList>
              <TabsTrigger value="all">Alle ({counts.all})</TabsTrigger>
              <TabsTrigger value="active">Aktive ({counts.active})</TabsTrigger>
              <TabsTrigger value="banned">Utestengt ({counts.banned})</TabsTrigger>
              <TabsTrigger value="unreliable">Upålitelig ({counts.unreliable})</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <RegisterChildDialog />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Barn</TableHead>
            <TableHead>Aldersgruppe</TableHead>
            <TableHead>Foresatt</TableHead>
            <TableHead>Status</TableHead>
            <TableHead>Aktive lån</TableHead>
            <TableHead>Sene returer</TableHead>
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

            return (
              <TableRow key={borrower.id}>
                <TableCell>
                  <div className="flex items-center gap-1.5 font-medium">
                    {borrower.name}
                    {borrower.isUnreliable && (
                      <AlertTriangle className="size-3.5 text-status-warning-fg" />
                    )}
                  </div>
                  <div className="text-xs text-muted-foreground">{age} år</div>
                </TableCell>
                <TableCell className="text-muted-foreground">{ageGroup(age) ?? '–'}</TableCell>
                <TableCell>{borrower.guardianName}</TableCell>
                <TableCell>
                  <StatusBadge tone={tone}>{label}</StatusBadge>
                </TableCell>
                <TableCell>{activeLoanCountByBorrower.get(borrower.id) ?? 0}</TableCell>
                <TableCell>{borrower.lateReturnCount}</TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
