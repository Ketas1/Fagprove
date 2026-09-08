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
import { Input } from '@/components/ui/input';
import type { Guardian } from '@/types/guardian';
import type { ProblemDetails } from '@/types/problem-details';

/**
 * Foresattes kontaktinformasjon brukes til oppfølging av forfalte lån, så den
 * må kunne rettes når et telefonnummer eller en e-postadresse endrer seg -
 * ellers stopper purringen opp. Se forretningsregel 5 i docs/03-domenemodell.md.
 */
export function EditGuardianDialog({
  guardian,
  open,
  onOpenChange,
}: {
  guardian: Guardian;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [name, setName] = useState(guardian.name);
  const [email, setEmail] = useState(guardian.email);
  const [phone, setPhone] = useState(guardian.phone);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/guardians/${guardian.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, email, phone }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Foresatt kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rediger foresatt</DialogTitle>
          <DialogDescription>
            Kontaktinformasjonen brukes til oppfølging av forfalte utlån.
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
              Telefon <span className="text-status-danger-fg">*</span>
            </label>
            <Input value={phone} onChange={(event) => setPhone(event.target.value)} />
          </div>

          <div className="flex flex-col gap-1.5">
            <label className="text-[12.5px] font-medium text-muted-foreground">
              E-post <span className="text-status-danger-fg">*</span>
            </label>
            <Input type="email" value={email} onChange={(event) => setEmail(event.target.value)} />
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Avbryt
          </Button>
          <Button
            onClick={handleSubmit}
            disabled={submitting || !name.trim() || !email.trim() || !phone.trim()}
          >
            <Check /> {submitting ? 'Lagrer …' : 'Lagre'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
