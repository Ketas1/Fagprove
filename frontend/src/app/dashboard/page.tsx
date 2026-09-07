import Link from 'next/link';
import { AlertTriangle, ArrowRight } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { NotBuiltYetBadge } from '@/components/not-built-yet';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { fetchBackend } from '@/lib/backend';
import { calculateAge } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import type { Borrower } from '@/types/borrower';
import type { Loan } from '@/types/loan';
import type { Equipment } from '@/types/equipment';

export const dynamic = 'force-dynamic';

export default async function OversiktPage() {
  const now = new Date();
  const [loans, borrowers, equipment] = await Promise.all([
    fetchBackend<Loan[]>(['loans']),
    fetchBackend<Borrower[]>(['borrowers']),
    fetchBackend<Equipment[]>(['equipment']),
  ]);

  const borrowersById = new Map(borrowers.map((borrower) => [borrower.id, borrower]));
  const withEffectiveStatus = loans.map((loan) => ({
    loan,
    status: effectiveLoanStatus(loan, now),
  }));

  const activeCount = withEffectiveStatus.filter(({ status }) => status === 'Active').length;
  const overdue = withEffectiveStatus.filter(({ status }) => status === 'Overdue');
  const availableCount = equipment.filter((item) => item.status === 'Available').length;
  const bannedCount = borrowers.filter((borrower) => borrower.status === 'Banned').length;

  return (
    <div className="flex flex-col gap-6">
      <div className="grid grid-cols-4 gap-4">
        <Card>
          <CardContent className="flex flex-col gap-1.5 pt-0">
            <span className="text-[12.5px] font-medium text-muted-foreground">Aktive utlån</span>
            <span className="text-[26px] font-semibold tracking-tight">{activeCount}</span>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex flex-col gap-1.5 pt-0">
            <span className="text-[12.5px] font-medium text-muted-foreground">Forfalte utlån</span>
            <span className="text-[26px] font-semibold tracking-tight text-status-danger-fg">
              {overdue.length}
            </span>
            <span className="text-xs text-status-danger-fg">krever oppfølging</span>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex flex-col gap-1.5 pt-0">
            <span className="text-[12.5px] font-medium text-muted-foreground">Ledig utstyr</span>
            <span className="text-[26px] font-semibold tracking-tight">
              {availableCount}{' '}
              <span className="text-sm font-medium text-muted-foreground">av {equipment.length}</span>
            </span>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="flex flex-col gap-1.5 pt-0">
            <span className="text-[12.5px] font-medium text-muted-foreground">Utestengte låntakere</span>
            <span className="text-[26px] font-semibold tracking-tight">{bannedCount}</span>
          </CardContent>
        </Card>
      </div>

      <div className="grid grid-cols-[1.7fr_1fr] items-start gap-6">
        <Card>
          <CardHeader className="flex-row items-center justify-between">
            <CardTitle>Utlån som krever oppfølging</CardTitle>
            <Link
              href="/dashboard/loans?status=Overdue"
              className="flex items-center gap-1 text-[12.5px] font-medium text-primary"
            >
              Se alle utlån <ArrowRight className="size-3.5" />
            </Link>
          </CardHeader>
          <CardContent>
            {overdue.length === 0 ? (
              <p className="py-6 text-center text-sm text-muted-foreground">
                Ingen utlån krever oppfølging akkurat nå.
              </p>
            ) : (
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Låntaker</TableHead>
                    <TableHead>Utstyr</TableHead>
                    <TableHead>Forfalt siden</TableHead>
                    <TableHead>Kontaktstatus</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {overdue.map(({ loan }) => {
                    const borrower = borrowersById.get(loan.borrowerId);
                    const daysOverdue = Math.floor(
                      (now.getTime() - new Date(loan.dueDate).getTime()) / (1000 * 60 * 60 * 24),
                    );

                    return (
                      <TableRow key={loan.id}>
                        <TableCell>
                          <Link href={`/dashboard/loans/${loan.id}`} className="font-medium hover:underline">
                            {loan.borrowerName}
                          </Link>
                          {borrower && (
                            <div className="text-xs text-muted-foreground">
                              {calculateAge(borrower.dateOfBirth, now)} år
                            </div>
                          )}
                        </TableCell>
                        <TableCell>{loan.equipmentName}</TableCell>
                        <TableCell>{daysOverdue} dager</TableCell>
                        <TableCell>
                          <NotBuiltYetBadge reason="Kontaktforsøk-logging finnes ikke i API-et ennå." />
                        </TableCell>
                      </TableRow>
                    );
                  })}
                </TableBody>
              </Table>
            )}
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex-row items-center justify-between">
            <CardTitle>Siste hendelser</CardTitle>
            <NotBuiltYetBadge reason="Krever et eget aktivitetslogg-endepunkt som ikke finnes i API-et ennå. Se docs/13-frontend-designsystem.md." />
          </CardHeader>
          <CardContent className="flex flex-col items-center gap-2 py-6 text-center text-sm text-muted-foreground">
            <AlertTriangle className="size-5" />
            Denne oversikten viser hendelser fra en aktivitetslogg som ikke er bygget ennå.
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
