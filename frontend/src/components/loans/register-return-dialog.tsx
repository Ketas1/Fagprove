'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Check, Clock } from 'lucide-react';
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { equipmentConditionLabel } from '@/lib/status-labels';
import type { EquipmentCondition } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

const CONDITIONS: EquipmentCondition[] = ['New', 'Good', 'Worn', 'Damaged'];

export function RegisterReturnDialog({
  loanId,
  equipmentName,
  isLate,
  daysOverdue,
}: {
  loanId: string;
  equipmentName: string;
  isLate: boolean;
  daysOverdue: number;
}) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [condition, setCondition] = useState<EquipmentCondition>('Good');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/loans/${loanId}/return`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ condition }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Returen kunne ikke registreres.' });
      return;
    }

    setOpen(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogTrigger render={<Button />}>
        <Check /> Registrer retur
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Registrer retur</DialogTitle>
          <DialogDescription>{equipmentName}</DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
            </div>
          )}

          {isLate && (
            <div className="flex gap-2.5 rounded-lg bg-status-warning-bg p-3 text-status-warning-fg">
              <Clock className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">
                Denne returen er <b>{daysOverdue} dager</b> forsinket. Låntakeren merkes automatisk som
                upålitelig, og telles i statistikken for sene returer.
              </div>
            </div>
          )}

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Tilstand ved retur <span className="text-status-danger-fg">*</span>
            </label>
            <Select value={condition} onValueChange={(value) => setCondition(value as EquipmentCondition)}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {CONDITIONS.map((value) => (
                  <SelectItem key={value} value={value}>
                    {equipmentConditionLabel(value)}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <p className="text-[11.5px] text-muted-foreground">
              Utstyret settes automatisk til «Ute av drift» ved skade.
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting}>
            <Check /> {submitting ? 'Registrerer …' : 'Bekreft retur'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
