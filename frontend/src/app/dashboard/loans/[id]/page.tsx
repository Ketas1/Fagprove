import Link from 'next/link';
import { notFound } from 'next/navigation';
import { ArrowLeft, Mail, Phone } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { LoanContactAttemptsSection } from '@/components/loans/loan-contact-attempts-section';
import { MarkLostButton } from '@/components/loans/mark-lost-button';
import { StatusBadge } from '@/components/status-badge';
import { RegisterReturnDialog } from '@/components/loans/register-return-dialog';
import { BackendError, fetchBackend } from '@/lib/backend';
import { calculateAge } from '@/lib/age';
import { effectiveLoanStatus } from '@/lib/loan-status';
import { borrowerStatusInfo, equipmentConditionLabel, equipmentStatusInfo, loanStatusInfo } from '@/lib/status-labels';
import type { Borrower } from '@/types/borrower';
import type { ContactAttempt } from '@/types/contact-attempt';
import type { Equipment } from '@/types/equipment';
import type { Guardian } from '@/types/guardian';
import type { Loan } from '@/types/loan';

export const dynamic = 'force-dynamic';

function formatDate(value: string): string {
  return new Date(value).toLocaleDateString('nb-NO');
}

export default async function LoanDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  let loan: Loan;

  try {
    loan = await fetchBackend<Loan>(['loans', id]);
  } catch (error) {
    if (error instanceof BackendError && error.status === 404) {
      notFound();
    }
    throw error;
  }

  const [borrower, equipment, contactAttempts] = await Promise.all([
    fetchBackend<Borrower>(['borrowers', loan.borrowerId]),
    fetchBackend<Equipment>(['equipment', loan.equipmentId]),
    fetchBackend<ContactAttempt[]>(['loans', id, 'contact-attempts']),
  ]);
  const guardian = await fetchBackend<Guardian>(['guardians', borrower.guardianId]);

  const now = new Date();
  const status = effectiveLoanStatus(loan, now);
  const { label: statusLabel, tone: statusTone } = loanStatusInfo(status);
  const daysOverdue = Math.max(
    0,
    Math.floor((now.getTime() - new Date(loan.dueDate).getTime()) / (1000 * 60 * 60 * 24)),
  );
  const canReturn = status === 'Active' || status === 'Overdue';

  return (
    <div className="flex flex-col gap-4">
      <Link
        href="/dashboard/loans"
        className="flex w-fit items-center gap-1.5 text-[12.5px] font-medium text-muted-foreground"
      >
        <ArrowLeft className="size-3.5" /> Tilbake til utlån
      </Link>

      <div className="flex items-start justify-between gap-4">
        <div>
          <div className="flex items-center gap-2.5">
            <h1 className="text-xl font-semibold tracking-tight">
              Utlån #{loan.id.slice(0, 8).toUpperCase()}
            </h1>
            <StatusBadge tone={statusTone} className="h-[26px] px-3 text-[12.5px]">
              {statusLabel}
            </StatusBadge>
          </div>
          <p className="mt-1 text-[12.5px] text-muted-foreground">
            {loan.equipmentName} til {loan.borrowerName} · registrert {formatDate(loan.startedAt)}
          </p>
        </div>
        <div className="flex items-center gap-2">
          {canReturn && (
            <>
              <MarkLostButton loanId={loan.id} />
              <RegisterReturnDialog
                loanId={loan.id}
                equipmentName={loan.equipmentName}
                isLate={status === 'Overdue'}
                daysOverdue={daysOverdue}
              />
            </>
          )}
        </div>
      </div>

      <div className="grid grid-cols-[1.65fr_1fr] items-start gap-5">
        <div className="flex flex-col gap-5">
          <Card>
            <CardHeader>
              <CardTitle>Detaljer om utlånet</CardTitle>
            </CardHeader>
            <CardContent className="grid grid-cols-3 gap-x-5 gap-y-4">
              <Field label="Startdato" value={formatDate(loan.startedAt)} />
              <Field
                label="Forventet returdato"
                value={formatDate(loan.dueDate)}
                valueClassName={status === 'Overdue' ? 'text-status-danger-fg' : undefined}
              />
              <Field
                label="Returtidspunkt"
                value={loan.returnedAt ? formatDate(loan.returnedAt) : 'Ikke levert'}
                valueClassName={loan.returnedAt ? undefined : 'font-normal text-muted-foreground'}
              />
              {loan.daysLate !== null && <Field label="Dager for sent levert" value={String(loan.daysLate)} />}
            </CardContent>
          </Card>

          <LoanContactAttemptsSection
            loanId={loan.id}
            initialAttempts={contactAttempts}
            isOverdue={status === 'Overdue'}
          />
        </div>

        <div className="flex flex-col gap-5">
          <Card>
            <CardHeader>
              <CardTitle>Låntaker</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2">
              <div className="text-[15px] font-semibold">{borrower.name}</div>
              <div className="text-xs text-muted-foreground">
                {calculateAge(borrower.dateOfBirth, now)} år
              </div>
              <div className="mt-1 flex items-center gap-1.5">
                {borrower.isUnreliable && <StatusBadge tone="warning">Upålitelig</StatusBadge>}
                <StatusBadge tone={borrowerStatusInfo(borrower.status).tone}>
                  {borrowerStatusInfo(borrower.status).label}
                </StatusBadge>
              </div>
              <div className="mt-2 flex items-center justify-between border-t pt-2.5 text-[12.5px]">
                <span className="text-muted-foreground">Sene returer</span>
                <span className="font-medium">{borrower.lateReturnCount}</span>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Foresatt</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2.5">
              <div className="text-[15px] font-semibold">{guardian.name}</div>
              <a
                href={`tel:${guardian.phone}`}
                className="flex items-center gap-2 text-[13px] text-foreground/90"
              >
                <Phone className="size-3.5 text-muted-foreground" /> {guardian.phone}
              </a>
              <a
                href={`mailto:${guardian.email}`}
                className="flex items-center gap-2 text-[13px] text-foreground/90"
              >
                <Mail className="size-3.5 text-muted-foreground" /> {guardian.email}
              </a>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Utstyr</CardTitle>
            </CardHeader>
            <CardContent className="flex flex-col gap-2.5">
              <div className="text-[15px] font-semibold">{equipment.name}</div>
              <div className="text-xs text-muted-foreground">
                {equipment.categoryName} · #{equipment.serialNumber}
              </div>
              <div className="flex items-center justify-between border-t pt-2.5 text-[12.5px]">
                <span className="text-muted-foreground">Tilstand</span>
                <span className="font-medium">{equipmentConditionLabel(equipment.condition)}</span>
              </div>
              <div className="flex items-center justify-between text-[12.5px]">
                <span className="text-muted-foreground">Status</span>
                <StatusBadge tone={equipmentStatusInfo(equipment.status).tone}>
                  {equipmentStatusInfo(equipment.status).label}
                </StatusBadge>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

function Field({
  label,
  value,
  valueClassName,
}: {
  label: string;
  value: string;
  valueClassName?: string;
}) {
  return (
    <div>
      <div className="text-[11.5px] font-medium text-muted-foreground">{label}</div>
      <div className={`mt-1 text-sm font-medium ${valueClassName ?? ''}`}>{value}</div>
    </div>
  );
}
