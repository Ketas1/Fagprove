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
import type { Equipment, EquipmentCategory } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

/**
 * Only Navn and Kategori are editable - `PUT /api/equipment/{id}` does not
 * accept Tilstand, see backend/SportForAlle.Api/Dtos/Equipment/UpdateEquipmentRequest.cs.
 * A condition field here would silently claim an ability the backend doesn't have.
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
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/equipment/${equipment.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, categoryId }),
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
          <DialogDescription>Navn og kategori kan endres. Serienummer og tilstand kan ikke.</DialogDescription>
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
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !name.trim() || !categoryId}>
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
