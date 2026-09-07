'use client';

import { useRouter } from 'next/navigation';
import { useState } from 'react';
import { AlertTriangle, Plus } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Textarea } from '@/components/ui/textarea';
import type { Note } from '@/types/note';
import type { ProblemDetails } from '@/types/problem-details';

function formatDateTime(value: string): string {
  return new Date(value).toLocaleString('nb-NO');
}

/**
 * Notes about a borrower - see the "saklige og faktabaserte" guidance in
 * docs/09-lover-og-regler.md. `initialNotes` comes from the server page;
 * adding a note calls `router.refresh()` to re-fetch, rather than appending
 * to local state, so the list always matches what the backend has.
 */
export function BorrowerNotesSection({ borrowerId, initialNotes }: { borrowerId: string; initialNotes: Note[] }) {
  const router = useRouter();
  const [text, setText] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [problem, setProblem] = useState<ProblemDetails | null>(null);

  async function handleSubmit() {
    setSubmitting(true);
    setProblem(null);

    const response = await fetch(`/api/borrowers/${borrowerId}/notes`, {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ text }),
    });

    setSubmitting(false);

    if (!response.ok) {
      const body = (await response.json().catch(() => null)) as ProblemDetails | null;
      setProblem(body ?? { detail: 'Notatet kunne ikke lagres.' });
      return;
    }

    setText('');
    router.refresh();
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Notater</CardTitle>
      </CardHeader>
      <CardContent className="flex flex-col gap-3">
        {problem && (
          <div className="flex gap-2.5 rounded-lg bg-status-danger-bg p-3 text-status-danger-fg">
            <AlertTriangle className="mt-0.5 size-4 shrink-0" />
            <div className="text-[12.5px] leading-relaxed">{problem.detail}</div>
          </div>
        )}

        {initialNotes.length === 0 ? (
          <p className="text-sm text-muted-foreground">Ingen notater ennå.</p>
        ) : (
          <ul className="flex flex-col gap-3">
            {initialNotes.map((note) => (
              <li key={note.id} className="rounded-lg border p-3 text-sm">
                <p>{note.text}</p>
                <p className="mt-1.5 text-[11.5px] text-muted-foreground">{formatDateTime(note.createdAt)}</p>
              </li>
            ))}
          </ul>
        )}

        <div className="flex flex-col gap-1.5 border-t pt-3">
          <Textarea
            value={text}
            onChange={(event) => setText(event.target.value)}
            placeholder="Skriv et saklig, faktabasert notat …"
          />
          <Button size="sm" className="w-fit" onClick={handleSubmit} disabled={submitting || !text.trim()}>
            <Plus /> {submitting ? 'Lagrer …' : 'Legg til notat'}
          </Button>
        </div>
      </CardContent>
    </Card>
  );
}
