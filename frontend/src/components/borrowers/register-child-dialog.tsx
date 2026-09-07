'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Check, Info, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Combobox } from '@/components/ui/combobox';
import { DatePicker } from '@/components/ui/date-picker';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import type { Guardian } from '@/types/guardian';
import type { ProblemDetails } from '@/types/problem-details';

type GuardianMode = 'new' | 'existing';

export function RegisterChildDialog({ guardians }: { guardians: Guardian[] }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [childName, setChildName] = useState('');
  const [dateOfBirth, setDateOfBirth] = useState('');
  const [guardianMode, setGuardianMode] = useState<GuardianMode>('new');
  const [existingGuardianId, setExistingGuardianId] = useState<string | null>(null);
  const [guardianName, setGuardianName] = useState('');
  const [guardianEmail, setGuardianEmail] = useState('');
  const [guardianPhone, setGuardianPhone] = useState('');
  const [identityVerified, setIdentityVerified] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  function reset() {
    setChildName('');
    setDateOfBirth('');
    setGuardianMode('new');
    setExistingGuardianId(null);
    setGuardianName('');
    setGuardianEmail('');
    setGuardianPhone('');
    setIdentityVerified(false);
    setProblem(null);
  }

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const body =
      guardianMode === 'existing'
        ? { name: childName, dateOfBirth, guardianId: existingGuardianId }
        : {
            name: childName,
            dateOfBirth,
            newGuardian: {
              name: guardianName,
              email: guardianEmail,
              phone: guardianPhone,
              identityVerified,
            },
          };

    const response = await fetch('/api/borrowers', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify(body),
    });

    setSubmitting(false);

    if (!response.ok) {
      const responseBody = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(responseBody ?? { detail: 'Barnet kunne ikke registreres.' });
      return;
    }

    setOpen(false);
    reset();
    router.refresh();
  }

  const guardianItems = guardians.map((guardian) => ({ id: guardian.id, label: `${guardian.name} · ${guardian.email}` }));

  const canSubmit =
    Boolean(childName) &&
    Boolean(dateOfBirth) &&
    (guardianMode === 'existing'
      ? Boolean(existingGuardianId)
      : Boolean(guardianName && guardianEmail && guardianPhone));

  return (
    <Dialog
      open={open}
      onOpenChange={(nextOpen) => {
        setOpen(nextOpen);
        if (!nextOpen) reset();
      }}
    >
      <DialogTrigger render={<Button />}>
        <Plus /> Registrer barn
      </DialogTrigger>
      <DialogContent className="sm:max-w-lg">
        <DialogHeader>
          <DialogTitle>Registrer barn</DialogTitle>
          <DialogDescription>Barnet knyttes til en foresatt for å kunne låne utstyr.</DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {problem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
            </div>
          )}

          <div className="flex items-center gap-2 text-[11px] font-semibold tracking-wide text-muted-foreground uppercase">
            Barn <span className="h-px flex-1 bg-border" />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">
                Navn <span className="text-status-danger-fg">*</span>
              </label>
              <Input value={childName} onChange={(event) => setChildName(event.target.value)} />
            </div>
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">
                Fødselsdato <span className="text-status-danger-fg">*</span>
              </label>
              <DatePicker value={dateOfBirth} onChange={setDateOfBirth} toYear={new Date().getFullYear()} />
            </div>
          </div>

          <div className="flex items-center gap-2 text-[11px] font-semibold tracking-wide text-muted-foreground uppercase">
            Foresatt <span className="h-px flex-1 bg-border" />
          </div>

          <Tabs value={guardianMode} onValueChange={(value) => setGuardianMode(value as GuardianMode)}>
            <TabsList>
              <TabsTrigger value="new">Ny foresatt</TabsTrigger>
              <TabsTrigger value="existing">Eksisterende foresatt</TabsTrigger>
            </TabsList>
          </Tabs>

          {guardianMode === 'existing' ? (
            <div className="flex flex-col gap-1.5">
              <label className="text-[12.5px] font-medium text-muted-foreground">
                Foresatt <span className="text-status-danger-fg">*</span>
              </label>
              <Combobox
                items={guardianItems}
                value={existingGuardianId}
                onChange={setExistingGuardianId}
                placeholder="Velg foresatt…"
                searchPlaceholder="Søk foresatt…"
                emptyText="Ingen foresatte funnet."
              />
            </div>
          ) : (
            <>
              <div className="flex flex-col gap-1.5">
                <label className="text-[12.5px] font-medium text-muted-foreground">
                  Navn <span className="text-status-danger-fg">*</span>
                </label>
                <Input value={guardianName} onChange={(event) => setGuardianName(event.target.value)} />
              </div>
              <div className="grid grid-cols-2 gap-4">
                <div className="flex flex-col gap-1.5">
                  <label className="text-[12.5px] font-medium text-muted-foreground">
                    Telefon <span className="text-status-danger-fg">*</span>
                  </label>
                  <Input value={guardianPhone} onChange={(event) => setGuardianPhone(event.target.value)} />
                </div>
                <div className="flex flex-col gap-1.5">
                  <label className="text-[12.5px] font-medium text-muted-foreground">
                    E-post <span className="text-status-danger-fg">*</span>
                  </label>
                  <Input
                    type="email"
                    value={guardianEmail}
                    onChange={(event) => setGuardianEmail(event.target.value)}
                  />
                </div>
              </div>
              <label className="flex items-start gap-2 text-[12.5px] text-muted-foreground">
                <Checkbox checked={identityVerified} onCheckedChange={setIdentityVerified} className="mt-0.5" />
                Identitet bekreftet (ID vist i butikken) - valgfritt, brukes i stedet for å lagre fødselsnummer
              </label>
            </>
          )}

          <div className="flex gap-2 rounded-lg bg-muted/60 p-2.5 text-xs text-muted-foreground">
            <Info className="mt-0.5 size-3.5 shrink-0" />
            Barnet kan ikke låne utstyr før foresatt er registrert og knyttet, slik det skjer her.
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Avbryt
          </Button>
          <Button onClick={handleSubmit} disabled={submitting || !canSubmit}>
            <Check /> {submitting ? 'Registrerer …' : 'Registrer barn'}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
