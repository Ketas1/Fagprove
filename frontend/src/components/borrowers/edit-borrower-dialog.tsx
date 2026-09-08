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
import { DatePicker } from '@/components/ui/date-picker';
import { Input } from '@/components/ui/input';
import type { Borrower } from '@/types/borrower';
import type { ProblemDetails } from '@/types/problem-details';

/**
 * Navn og fødselsdato kan endres. Foresatt kan ikke - å flytte et barn til en
 * annen foresatt er en annen operasjon enn å rette en skrivefeil, og har
 * ingen endepunkt.
 *
 * Å endre fødselsdatoen endrer også historiske aldersgrupperapporter, fordi
 * rapporten regner ut alderen fra fødselsdatoen ved lånets startdato hver
 * gang den kjøres. Det er tilsiktet: var datoen feil, var tallene det også.
 * Se ADR-0024.
 */
export function EditBorrowerDialog({
  borrower,
  open,
  onOpenChange,
}: {
  borrower: Borrower;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [name, setName] = useState(borrower.name);
  const [dateOfBirth, setDateOfBirth] = useState(borrower.dateOfBirth);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  const dateChanged = dateOfBirth !== borrower.dateOfBirth;

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/borrowers/${borrower.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, dateOfBirth }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Barnet kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rediger barn</DialogTitle>
          <DialogDescription>Navn og fødselsdato kan endres. Foresatt kan ikke.</DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Navn <span className="text-status-danger-fg">*</span>
            </label>
            <Input value={name} onChange={(event) => setName(event.target.value)} autoFocus />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Fødselsdato <span className="text-status-danger-fg">*</span>
            </label>
            <DatePicker
              value={dateOfBirth}
              onChange={setDateOfBirth}
              toYear={new Date().getFullYear()}
            />
            {dateChanged && (
              <p className="flex gap-2 rounded-lg bg-status-warning-bg p-2.5 text-[11.5px] leading-relaxed text-status-warning-fg">
                <AlertTriangle className="mt-0.5 size-3.5 shrink-0" />
                Endrer du fødselsdatoen, flyttes barnets tidligere utlån til en annen aldersgruppe i
                rapportene.
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !name.trim() || !dateOfBirth}>
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
