'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, PackageX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import type { ProblemDetails } from '@/types/problem-details';

/**
 * Confirms before firing - this is a terminal state transition (the loan
 * becomes `Lost`, the equipment `WrittenOff`, see docs/03-domenemodell.md),
 * not something a misclick should be able to trigger directly.
 */
export function MarkLostButton({ loanId }: { loanId: string }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleConfirm() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/loans/${loanId}/mark-lost`, { method: 'POST' });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kunne ikke registrere tap/skade.' });
      return;
    }

    setOpen(false);
    router.refresh();
  }

  return (
    <>
      <Button variant="outline" onClick={() => setOpen(true)}>
        <PackageX /> Marker som tapt/ødelagt
      </Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Bekreft tap eller skade</DialogTitle>
            <DialogDescription>
              Dette avslutter lånet permanent og setter utstyret til «Avskrevet». Kan ikke angres.
            </DialogDescription>
          </DialogHeader>
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setOpen(false)}>
              Avbryt
            </Button>
            <Button variant="destructive" onClick={handleConfirm} disabled={submitting}>
              <PackageX /> {submitting ? 'Registrerer …' : 'Bekreft tap/skade'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
