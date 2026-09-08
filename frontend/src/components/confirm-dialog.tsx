'use client';

import { useRouter } from 'next/navigation';
import { useState, type ReactNode } from 'react';
import { AlertTriangle } from 'lucide-react';
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
 * Confirmation step in front of an action that destroys or hides data.
 * Deleting a child, anonymising a guardian and removing an employee are all
 * irreversible or near-irreversible, and none of them should be one stray
 * click away - see ADR-0026.
 *
 * `action` returns the raw `Response` so the dialog can show the backend's
 * RFC 7807 `detail` verbatim. That matters here: a refusal is usually a rule
 * doing its job ("Låntakeren har historikk og kan ikke slettes"), and the
 * staff member needs to read exactly that, not a generic failure.
 */
export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel,
  destructive = false,
  action,
  children,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: ReactNode;
  confirmLabel: string;
  /** Red confirm button, for anything that cannot be undone. */
  destructive?: boolean;
  action: () => Promise<Response>;
  /** Optional extra content between the description and the buttons. */
  children?: ReactNode;
}) {
  const router = useRouter();
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleConfirm() {
    setSubmitting(true);
    setProblem(null);

    const response = await action();
    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Handlingen kunne ikke utføres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>

        {(problem || children) && (
          <div className="flex flex-col gap-4">
            {problem && (
              <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
                <AlertTriangle className="mt-0.5 size-4 shrink-0" />
                <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
              </div>
            )}
            {children}
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button
            variant={destructive ? 'destructive' : 'default'}
            onClick={handleConfirm}
            disabled={submitting}
          >
            {submitting ? 'Utfører …' : confirmLabel}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
