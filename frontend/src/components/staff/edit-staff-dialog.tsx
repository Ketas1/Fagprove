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
import type { ProblemDetails } from '@/types/problem-details';
import type { Staff } from '@/types/staff';

/**
 * Stilling er en fritekstbeskrivelse av rollen i butikken, ikke en
 * tilgangsrolle - tilganger styres av Auth0, se ADR-0019. Kobling til
 * Auth0-konto endres ikke her; den ansatte kobler sin egen profil selv.
 */
export function EditStaffDialog({
  staff,
  open,
  onOpenChange,
}: {
  staff: Staff;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const router = useRouter();
  const [name, setName] = useState(staff.name);
  const [jobTitle, setJobTitle] = useState(staff.jobTitle ?? '');
  const [email, setEmail] = useState(staff.email ?? '');
  const [phone, setPhone] = useState(staff.phone ?? '');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/staff/${staff.id}`, {
      method: 'PUT',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name, jobTitle, email, phone }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Ansatt kunne ikke endres.' });
      return;
    }

    onOpenChange(false);
    router.refresh();
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>Rediger ansatt</DialogTitle>
          <DialogDescription>
            Stilling, e-post og telefon er valgfrie. Tomt felt fjerner det som er lagret.
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
            <label className="text-[12.5px] font-medium text-muted-foreground">Stilling</label>
            <Input
              value={jobTitle}
              onChange={(event) => setJobTitle(event.target.value)}
              placeholder="Butikkleder"
            />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">Telefon</label>
              <Input value={phone} onChange={(event) => setPhone(event.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">E-post</label>
              <Input
                type="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="navn@sportforalle.no"
              />
            </div>
          </div>
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
