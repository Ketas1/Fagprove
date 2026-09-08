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
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { equipmentConditionLabel } from '@/lib/status-labels';
import type { Equipment, EquipmentCategory, EquipmentCondition } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

const CONDITIONS: EquipmentCondition[] = ['New', 'Good', 'Worn', 'Damaged'];

/**
 * Navn, kategori, serienummer og tilstand kan endres. **Status kan ikke** -
 * den eies av Utstyrstatus-tilstandsmaskinen i docs/03-domenemodell.md og
 * flyttes bare av utlån, retur, reparasjon eller avskriving. Et statusfelt
 * her ville latt en ansatt sette utstyr som er utlånt tilbake til ledig.
 */
export function EditEquipmentDialog({
  equipment,
  categories,
  open,
  onOpenChange,
}: {
  equipment: Equipment;
  categories: EquipmentCategory[];
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [name, setName] = useState(equipment.name);
  const [categoryId, setCategoryId] = useState(equipment.categoryId);
  const [serialNumber, setSerialNumber] = useState(equipment.serialNumber);
  const [condition, setCondition] = useState<EquipmentCondition>(equipment.condition);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/equipment/${equipment.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, categoryId, serialNumber, condition }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Utstyret kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rediger utstyr</DialogTitle>
          <DialogDescription>
            Status endres ikke her - den følger av utlån, retur og avskriving.
          </DialogDescription>
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
              Kategori <span className="text-status-danger-fg">*</span>
            </label>
            <Combobox
              items={categories.map((category) => ({ id: category.id, label: category.name }))}
              value={categoryId || null}
              onChange={(value) => setCategoryId(value ?? '')}
              placeholder="Velg kategori…"
              searchPlaceholder="Søk kategori…"
              emptyText="Ingen kategorier funnet."
            />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Serienummer <span className="text-status-danger-fg">*</span>
            </label>
            <Input value={serialNumber} onChange={(event) => setSerialNumber(event.target.value)} />
            <p className="text-[11.5px] text-muted-foreground">
              Må være unikt. Er nummeret allerede i bruk, avvises lagringen.
            </p>
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Tilstand <span className="text-status-danger-fg">*</span>
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
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={submitting || !name.trim() || !categoryId || !serialNumber.trim()}
          >
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
