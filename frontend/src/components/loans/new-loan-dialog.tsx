'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Check, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Combobox } from '@/components/ui/combobox';
import { calculateAge } from '@/lib/age';
import type { Borrower } from '@/types/borrower';
import type { Equipment } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

function tomorrowIsoDate(): string {
  const tomorrow = new Date();
  tomorrow.setDate(tomorrow.getDate() + 1);
  return tomorrow.toISOString().slice(0, 10);
}

export function NewLoanDialog({ borrowers, equipment }: { borrowers: Borrower[]; equipment: Equipment[] }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [borrowerId, setBorrowerId] = useState('');
  const [equipmentId, setEquipmentId] = useState('');
  const [dueDate, setDueDate] = useState(tomorrowIsoDate());
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  const borrowableBorrowers = borrowers.filter((borrower) => borrower.status !== 'Banned');
  const availableEquipment = equipment.filter((item) => item.status === 'Available');

  const borrowerItems = borrowableBorrowers.map((borrower) => ({
    id: borrower.id,
    label: `${borrower.name} · ${calculateAge(borrower.dateOfBirth, new Date())} år`,
  }));
  const equipmentItems = availableEquipment.map((item) => ({
    id: item.id,
    label: `${item.name} · #${item.serialNumber}`,
    group: item.categoryName,
  }));

  function reset() {
    setBorrowerId('');
    setEquipmentId('');
    setDueDate(tomorrowIsoDate());
    setProblem(null);
  }

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch('/api/loans', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({
        borrowerId,
        equipmentId,
        dueDate: new Date(`${dueDate}T23:59:59`).toISOString(),
      }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Utlånet kunne ikke registreres.' });
      return;
    }

    setOpen(false);
    reset();
    router.refresh();
  }

  return (
    <Dialog
      open={open}
      onOpenChange={(nextOpen) => {
        setOpen(nextOpen);
        if (!nextOpen) reset();
      }}
    >
      <DialogTrigger render={<Button />}>
        <Plus /> Nytt utlån
      </DialogTrigger>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Nytt utlån</DialogTitle>
          <DialogDescription>Registrer utlån av utstyr til en låntaker.</DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div>
                <div className="text-sm font-semibold">Utlån blokkert</div>
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
              placeholder="Velg ledig utstyr…"
              searchPlaceholder="Søk utstyr…"
              emptyText="Ingen ledig utstyr funnet."
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="new-loan-due-date" className="text-[12.5px] font-medium text-muted-foreground">
              Forventet returdato <span className="text-status-danger-fg">*</span>
            </label>
            <input
              id="new-loan-due-date"
              type="date"
              value={dueDate}
              min={tomorrowIsoDate()}
              onChange={(event) => setDueDate(event.target.value)}
              className="h-9 rounded-md border border-input bg-transparent px-3 text-sm outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !borrowerId || !equipmentId || !dueDate}>
            <Check /> {submitting ? 'Registrerer …' : 'Registrer utlån'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
