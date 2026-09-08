'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Check } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Combobox } from '@/components/ui/combobox';
import { calculateAge } from '@/lib/age';
import type { Borrower } from '@/types/borrower';
import type { Equipment } from '@/types/equipment';
import type { Loan } from '@/types/loan';
import type { ProblemDetails } from '@/types/problem-details';

/** "2026-09-08T10:00:00+02:00" -> "2026-09-08", for a date input. */
function toDateInput(value: string): string {
  const date = new Date(value);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Retter et åpent utlån som ble registrert med feil opplysninger. Bare
 * aktive og forfalte utlån kan endres - et levert eller tapt utlån er
 * historikk rapportene bygger på, og backend avviser det med
 * `409 LoanAlreadyClosed`. Se ADR-0025.
 *
 * Bytter man utstyr, frigis det forrige og det nye settes som utlånt i samme
 * operasjon. Bytter man låntaker, kjøres forretningsregel 2 på nytt mot den
 * nye - er hen utestengt eller har et forfalt lån, avvises endringen.
 */
export function EditLoanDialog({
  loan,
  borrowers,
  equipment,
  open,
  onOpenChange,
}: {
  loan: Loan;
  borrowers: Borrower[];
  equipment: Equipment[];
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [borrowerId, setBorrowerId] = useState(loan.borrowerId);
  const [equipmentId, setEquipmentId] = useState(loan.equipmentId);
  const [startedAt, setStartedAt] = useState(toDateInput(loan.startedAt));
  const [dueDate, setDueDate] = useState(toDateInput(loan.dueDate));
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  const borrowerItems = borrowers
    .filter((borrower) => borrower.status !== 'Banned' || borrower.id === loan.borrowerId)
    .map((borrower) => ({
      id: borrower.id,
      label: `${borrower.name} · ${calculateAge(borrower.dateOfBirth, new Date())} år`,
    }));

  // The loan's own item is OnLoan because of this loan, so it has to stay
  // selectable alongside the genuinely available ones.
  const equipmentItems = equipment
    .filter((item) => item.status === 'Available' || item.id === loan.equipmentId)
    .map((item) => ({
      id: item.id,
      label: `${item.name} · #${item.serialNumber}`,
      group: item.categoryName,
    }));

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/loans/${loan.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({
        borrowerId,
        equipmentId,
        // Same convention as NewLoanDialog: a date input carries no time, so
        // the start is pinned to the beginning of that local day and the due
        // date to the end of it, then both are sent as UTC. Sending the bare
        // "yyyy-MM-dd" would bind to a +02:00 offset, which PostgreSQL
        // rejects for a 'timestamp with time zone'.
        startedAt: new Date(`${startedAt}T00:00:00`).toISOString(),
        dueDate: new Date(`${dueDate}T23:59:59`).toISOString(),
      }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Utlånet kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rediger utlån</DialogTitle>
          <DialogDescription>
            Bytter du utstyr, frigis det forrige automatisk.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div>
                <div className="text-sm font-semibold">Endringen ble blokkert</div>
                <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
              </div>
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Låntaker <span className="text-status-danger-fg">*</span>
            </label>
            <Combobox
              items={borrowerItems}
              value={borrowerId || null}
              onChange={(value) => setBorrowerId(value ?? '')}
              placeholder="Velg låntaker…"
              searchPlaceholder="Søk låntaker…"
              emptyText="Ingen låntakere funnet."
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Utstyr <span className="text-status-danger-fg">*</span>
            </label>
            <Combobox
              items={equipmentItems}
              value={equipmentId || null}
              onChange={(value) => setEquipmentId(value ?? '')}
              placeholder="Velg utstyr…"
              searchPlaceholder="Søk utstyr…"
              emptyText="Ingen ledig utstyr funnet."
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="edit-loan-started" className="text-[12.5px] font-medium text-muted-foreground">
                Startdato <span className="text-status-danger-fg">*</span>
              </label>
              <input
                id="edit-loan-started"
                type="date"
                value={startedAt}
                onChange={(event) => setStartedAt(event.target.value)}
                className="h-9 rounded-md border border-input bg-transparent px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
              />
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="edit-loan-due" className="text-[12.5px] font-medium text-muted-foreground">
                Forventet returdato <span className="text-status-danger-fg">*</span>
              </label>
              <input
                id="edit-loan-due"
                type="date"
                value={dueDate}
                onChange={(event) => setDueDate(event.target.value)}
                className="h-9 rounded-md border border-input bg-transparent px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
              />
            </div>
          </div>

          {dueDate <= startedAt && (
            <p className="text-[11.5px] text-status-danger-fg">
              Returdatoen må være etter startdatoen.
            </p>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={submitting || !borrowerId || !equipmentId || !startedAt || dueDate <= startedAt}
          >
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
