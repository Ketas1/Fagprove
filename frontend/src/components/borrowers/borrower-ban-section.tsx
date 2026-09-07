'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Ban as BanIcon, Check } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import type { Ban } from '@/types/ban';
import type { BorrowerStatus } from '@/types/borrower';
import type { ProblemDetails } from '@/types/problem-details';

/**
 * Utestengelse (ban) lifecycle for one borrower - business rules 2 and 8 in
 * docs/03-domenemodell.md. `initialBan` comes from the server page (fetched
 * only when `status === 'Banned'`, since there is nothing to show
 * otherwise); every mutation here calls `router.refresh()` rather than
 * managing local state, so the page's own server-fetched `status`/`ban` stay
 * the single source of truth.
 */
export function BorrowerBanSection({
  borrowerId,
  status,
  initialBan,
}: {
  borrowerId: string;
  status: BorrowerStatus;
  initialBan: Ban | null;
}) {
  const router = useRouter();
  const [banDialogOpen, setBanDialogOpen] = useState(false);
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleBan() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/borrowers/${borrowerId}/ban`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ reason }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kunne ikke utestenge låntakeren.' });
      return;
    }

    setBanDialogOpen(false);
    setReason('');
    router.refresh();
  }

  async function handleFeePaid() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/borrowers/${borrowerId}/ban/fee-paid`, { method: 'POST' });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kunne ikke registrere gebyrbetaling.' });
      return;
    }

    router.refresh();
  }

  async function handleLiftBan() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/borrowers/${borrowerId}/ban`, { method: 'DELETE' });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kunne ikke oppheve utestengelsen.' });
      return;
    }

    router.refresh();
  }

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between">
        <CardTitle>Utestengelse</CardTitle>
        {status === 'Active' && (
          <Button variant="outline" size="sm" onClick={() => setBanDialogOpen(true)}>
            <BanIcon /> Utesteng låntaker
          </Button>
        )}
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {problem && (
          <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
          </div>
        )}

        {status === 'Active' && <p className="text-sm text-muted-foreground">Låntakeren er ikke utestengt.</p>}

        {status === 'Banned' && initialBan && (
          <>
            <div>
              <div className="text-[11.5px] font-medium text-muted-foreground">Årsak</div>
              <div className="mt-1 text-sm">{initialBan.reason}</div>
            </div>
            <div className="flex items-center justify-between border-t pt-3 text-[12.5px]">
              <span className="text-muted-foreground">Gebyr</span>
              <span className="font-medium">{initialBan.feePaidAt ? 'Betalt' : 'Ikke betalt'}</span>
            </div>
            <div className="flex gap-2">
              {!initialBan.feePaidAt && (
                <Button variant="outline" size="sm" onClick={handleFeePaid} disabled={submitting}>
                  Registrer gebyr betalt
                </Button>
              )}
              <Button
                variant="outline"
                size="sm"
                onClick={handleLiftBan}
                disabled={submitting || !initialBan.feePaidAt}
              >
                <Check /> Opphev utestengelse
              </Button>
            </div>
          </>
        )}
      </CardContent>

      <Dialog open={banDialogOpen} onOpenChange={setBanDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Utesteng låntaker</DialogTitle>
            <DialogDescription>Årsaken lagres og kan forklares til foresatt ved behov.</DialogDescription>
          </DialogHeader>
          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              Årsak <span className="text-status-danger-fg">*</span>
            </label>
            <Textarea value={reason} onChange={(event) => setReason(event.target.value)} autoFocus />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setBanDialogOpen(false)}>
              Avbryt
            </Button>
            <Button onClick={handleBan} disabled={submitting || !reason.trim()}>
              <BanIcon /> {submitting ? 'Utestenger …' : 'Utesteng'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
}
