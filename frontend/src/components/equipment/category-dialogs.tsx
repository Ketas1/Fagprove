'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Check, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import type { EquipmentCategory } from '@/types/equipment';
import type { ProblemDetails } from '@/types/problem-details';

/** Creates either a top-level category (parentCategoryId null) or a subcategory of parentName. */
export function CreateCategoryDialog({
  open,
  onOpenChange,
  parentCategoryId,
  parentName,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  parentCategoryId: string | null;
  parentName: string | null;
}) {
  const router = useRouter();
  const [name, setName] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch('/api/equipment-categories', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, parentCategoryId: parentCategoryId ?? undefined }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kategorien kunne ikke opprettes.' });
      return;
    }

    setName('');
    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Ny kategori</DialogTitle>
          <DialogDescription>
            {parentName ? `Legges til under «${parentName}»` : 'Legges til som toppnivå-kategori'}
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
            <label className="text-[12.5px] font-medium text-muted-foreground">Navn</label>
            <Input
              value={name}
              onChange={(event) => setName(event.target.value)}
              placeholder="F.eks. Ski"
              autoFocus
            />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !name.trim()}>
            <Check /> {submitting ? 'Oppretter …' : 'Opprett'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function RenameCategoryDialog({
  category,
  open,
  onOpenChange,
}: {
  category: EquipmentCategory;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [name, setName] = useState(category.name);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/equipment-categories/${category.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kategorien kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Gi nytt navn</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
            </div>
          )}
          <Input value={name} onChange={(event) => setName(event.target.value)} autoFocus />
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !name.trim()}>
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

export function DeleteCategoryDialog({
  category,
  open,
  onOpenChange,
  onDeleted,
}: {
  category: EquipmentCategory;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onDeleted: () => void;
}) {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleDelete() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/equipment-categories/${category.id}`, { method: 'DELETE' });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kategorien kunne ikke slettes.' });
      return;
    }

    onOpenChange(false);
    onDeleted();
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Slett «{category.name}»?</DialogTitle>
          <DialogDescription>Dette kan ikke angres.</DialogDescription>
        </DialogHeader>

        {problem && (
          <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button variant="destructive" onClick={handleDelete} disabled={submitting}>
            <Trash2 /> {submitting ? 'Sletter …' : 'Slett'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
