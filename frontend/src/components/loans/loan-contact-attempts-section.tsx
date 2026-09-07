'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Mail, Send } from 'lucide-react';
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
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import type { ContactAttempt, ContactMethod } from '@/types/contact-attempt';
import type { ProblemDetails } from '@/types/problem-details';

const METHODS: ContactMethod[] = ['Phone', 'Email'];
const METHOD_LABELS: Record<ContactMethod, string> = { Email: 'E-post', Phone: 'Telefon' };

function formatDateTime(value: string): string {
  return new Date(value).toLocaleString('nb-NO');
}

/**
 * Business rule 6 in docs/03-domenemodell.md: every contact attempt with a
 * guardian is logged, so staff can see whether they've already been
 * contacted. `initialAttempts` comes from the server page; logging one
 * calls `router.refresh()` to re-fetch rather than appending locally.
 */
/**
 * `isOverdue` gates the "Send oppfølgings-e-post" action - the backend
 * rejects it for a loan that isn't overdue (409 LoanNotOverdue) anyway, see
 * docs/adr/0022-emailjs-server-side.md, but hiding the button avoids a
 * pointless round-trip and confusion for staff.
 */
export function LoanContactAttemptsSection({
  loanId,
  initialAttempts,
  isOverdue,
}: {
  loanId: string;
  initialAttempts: ContactAttempt[];
  isOverdue: boolean;
}) {
  const router = useRouter();
  const [method, setMethod] = useState<ContactMethod>('Phone');
  const [outcome, setOutcome] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);
  const [emailDialogOpen, setEmailDialogOpen] = useState(false);
  const [sendingEmail, setSendingEmail] = useState(false);
  const [emailProblem, setEmailProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/loans/${loanId}/contact-attempts`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ method, outcome }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Kontaktforsøket kunne ikke lagres.' });
      return;
    }

    setOutcome('');
    router.refresh();
  }

  async function handleSendEmail() {
    setSendingEmail(true);
    setEmailProblem(null);

    const response = await fetch(`/api/loans/${loanId}/send-followup-email`, { method: 'POST' });

    setSendingEmail(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setEmailProblem(body ?? { detail: 'E-posten kunne ikke sendes.' });
      return;
    }

    setEmailDialogOpen(false);
    router.refresh();
  }

  return (
    <Card>
      <CardHeader className="flex-row items-center justify-between">
        <CardTitle>Kommunikasjon med foresatt</CardTitle>
        {isOverdue && (
          <Button variant="outline" size="sm" onClick={() => setEmailDialogOpen(true)}>
            <Mail /> Send oppfølgings-e-post
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

        {initialAttempts.length === 0 ? (
          <p className="text-sm text-muted-foreground">Ingen kontaktforsøk registrert ennå.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {initialAttempts.map((attempt) => (
              <li key={attempt.id} className="rounded-lg border p-3 text-sm">
                <div className="flex items-center justify-between">
                  <span className="font-medium">{METHOD_LABELS[attempt.method]}</span>
                  <span className="text-[11.5px] text-muted-foreground">{formatDateTime(attempt.createdAt)}</span>
                </div>
                <p className="mt-1 text-muted-foreground">{attempt.outcome}</p>
              </li>
            ))}
          </ul>
        )}

        <div className="flex flex-col gap-2 border-t pt-3">
          <div className="flex gap-2">
            <Select value={method} onValueChange={(value) => setMethod(value as ContactMethod)}>
              <SelectTrigger className="w-32">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {METHODS.map((value) => (
                  <SelectItem key={value} value={value}>
                    {METHOD_LABELS[value]}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Textarea
              value={outcome}
              onChange={(event) => setOutcome(event.target.value)}
              placeholder="Resultat av kontaktforsøket …"
              className="flex-1"
            />
          </div>
          <Button size="sm" className="w-fit" onClick={handleSubmit} disabled={submitting || !outcome.trim()}>
            <Send /> {submitting ? 'Logger …' : 'Logg kontaktforsøk'}
          </Button>
        </div>
      </CardContent>

      <Dialog open={emailDialogOpen} onOpenChange={setEmailDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Send oppfølgings-e-post</DialogTitle>
            <DialogDescription>
              Sender en e-post til foresatt om det forfalte lånet, og logger den automatisk som et
              kontaktforsøk.
            </DialogDescription>
          </DialogHeader>
          {emailProblem && (
            <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
              <AlertTriangle className="mt-0.5 size-4 shrink-0" />
              <div className="text-[12.5px] leading-relaxed">{emailProblem.detail}</div>
            </div>
          )}
          <DialogFooter>
            <Button variant="outline" onClick={() => setEmailDialogOpen(false)}>
              Avbryt
            </Button>
            <Button onClick={handleSendEmail} disabled={sendingEmail}>
              <Mail /> {sendingEmail ? 'Sender …' : 'Send e-post'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </Card>
  );
}
