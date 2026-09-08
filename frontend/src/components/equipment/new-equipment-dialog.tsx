'use client';

import { useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';
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
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { equipmentConditionLabel } from '@/lib/status-labels';
import type { EquipmentCategory, EquipmentCondition } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

const CONDITIONS: EquipmentCondition[] = ['New', 'Good', 'Worn', 'Damaged'];

export function NewEquipmentDialog({
  categories,
  initialCategoryId,
}: {
  categories: EquipmentCategory[];
  initialCategoryId?: string | null;
}) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [name, setName] = useState('');
  const [serialNumber, setSerialNumber] = useState('');
  const [categoryId, setCategoryId] = useState(initialCategoryId ?? '');
  const [condition, setCondition] = useState<EquipmentCondition>('New');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);
  const [extraCategories, setExtraCategories] = useState<EquipmentCategory[]>([]);
  const [creatingCategory, setCreatingCategory] = useState(false);

  // Newly created categories are merged in immediately so the just-created
  // one shows up without waiting for router.refresh() to land - deduped by
  // id in case the refresh arrives while this dialog is still open.
  const allCategories = useMemo(() => {
    const byId = new Map(categories.map((category) => [category.id, category]));
    for (const extra of extraCategories) {
      if (!byId.has(extra.id)) {
        byId.set(extra.id, extra);
      }
    }
    return [...byId.values()];
  }, [categories, extraCategories]);

  function reset() {
    setName('');
    setSerialNumber('');
    setCategoryId(initialCategoryId ?? '');
    setCondition('New');
    setProblem(null);
    setExtraCategories([]);
  }

  /** The category Combobox's "create new" shortcut - always a top-level category, matching CreateCategoryDialog's own default. */
  async function handleCreateCategory(categoryName: string) {
    setCreatingCategory(true);
    setProblem(null);

    const response = await fetch('/api/equipment-categories', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name: categoryName }),
    });

    setCreatingCategory(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kategorien kunne ikke opprettes.' });
      return;
    }

    const created = (await response.json()) as EquipmentCategory;
    setExtraCategories((prev) => [...prev, created]);
    setCategoryId(created.id);
    router.refresh();
  }

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch('/api/equipment', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, serialNumber, categoryId, condition }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Utstyret kunne ikke legges til.' });
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
        reset();
      }}
    >
      <DialogTrigger render={<Button />}>
        <Plus /> Nytt utstyr
      </DialogTrigger>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Nytt utstyr</DialogTitle>
          <DialogDescription>Legg til utstyr i lagerbeholdningen.</DialogDescription>
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
            <Input value={name} onChange={(event) => setName(event.target.value)} placeholder="Snowboard, str. 155" />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">
                Kategori <span className="text-status-danger-fg">*</span>
              </label>
              <Combobox
                items={allCategories.map((category) => ({ id: category.id, label: category.name }))}
                value={categoryId || null}
                onChange={(value) => setCategoryId(value ?? '')}
                placeholder="Velg kategori…"
                searchPlaceholder="Søk kategori…"
                emptyText="Ingen kategorier funnet."
                onCreateNew={handleCreateCategory}
                createLabel={(query) => `Opprett kategori «${query}»`}
                disabled={creatingCategory}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">
                Serienummer <span className="text-status-danger-fg">*</span>
              </label>
              <Input
                value={serialNumber}
                onChange={(event) => setSerialNumber(event.target.value)}
                placeholder="SF-0411"
              />
            </div>
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">Tilstand</label>
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
          <Button variant="outline" onClick={() => setOpen(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !name || !serialNumber || !categoryId}>
            <Check /> {submitting ? 'Legger til …' : 'Legg til utstyr'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
