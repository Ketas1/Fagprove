'use client';

import { useEffect, useState } from 'react';
import type { Staff } from '@/types/staff';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';

type State =
  | { status: 'loading' }
  | { status: 'linked'; me: Staff }
  | { status: 'unlinked'; staff: Staff[] }
  | { status: 'error' };

/**
 * Lets the logged-in user create a Staff profile and link their own Auth0
 * account to one, through the same-origin proxy - see
 * docs/adr/0019-staff-auth0-mapping.md. Every /api/* endpoint requires a
 * linked Staff profile except this one's endpoints, which exist for exactly
 * this bootstrapping step.
 *
 * Checks `/api/staff/me` first: once linked, there is nothing left to do
 * here, so the full bootstrap list (every staff member's name and link
 * status) only renders for someone who genuinely still needs it - see
 * `GET /api/staff/me` in docs/05-api.md.
 */
export function StaffLinkPanel() {
  const [state, setState] = useState<State>({ status: 'loading' });
  const [name, setName] = useState('');
  const [message, setMessage] = useState<string | null>(null);
  const [reloadToken, setReloadToken] = useState(0);

  // Mirrors StatusWidget's fetch-in-effect shape: setState only happens
  // inside the .then()/.catch() callbacks, never synchronously in the
  // effect body. `reloadToken` is bumped after a mutation to trigger a
  // refetch, rather than calling a shared async function directly from the
  // effect and from event handlers.
  useEffect(() => {
    const controller = new AbortController();

    fetch('/api/staff/me', { cache: 'no-store', signal: controller.signal })
      .then(async (meResponse) => {
        if (meResponse.ok) {
          const me = (await meResponse.json()) as Staff;
          setState({ status: 'linked', me });
          return;
        }

        if (meResponse.status !== 404) {
          setState({ status: 'error' });
          return;
        }

        const listResponse = await fetch('/api/staff', { cache: 'no-store', signal: controller.signal });

        if (!listResponse.ok) {
          setState({ status: 'error' });
          return;
        }

        const staff = (await listResponse.json()) as Staff[];
        setState({ status: 'unlinked', staff });
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return;
        }

        setState({ status: 'error' });
      });

    return () => controller.abort();
  }, [reloadToken]);

  async function handleCreate(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setMessage(null);

    const response = await fetch('/api/staff', {
      method: 'POST',
      headers: { 'content-type': 'application/json' },
      body: JSON.stringify({ name }),
    });

    if (!response.ok) {
      setMessage('Kunne ikke opprette profilen.');
      return;
    }

    setName('');
    setReloadToken((token) => token + 1);
  }

  async function handleLink(id: string) {
    setMessage(null);

    const response = await fetch(`/api/staff/${id}/link-me`, { method: 'POST' });

    if (!response.ok) {
      const problem = (await response.json().catch(() => null)) as { reason?: string } | null;

      if (problem?.reason === 'StaffAlreadyLinked') {
        setMessage('Denne profilen er allerede koblet til en annen konto.');
      } else if (problem?.reason === 'Auth0AccountAlreadyLinked') {
        setMessage('Kontoen din er allerede koblet til en annen profil.');
      } else {
        setMessage('Kunne ikke koble kontoen til profilen.');
      }

      return;
    }

    setReloadToken((token) => token + 1);
  }

  if (state.status === 'loading') {
    return <p className="text-muted-foreground text-sm">Laster...</p>;
  }

  if (state.status === 'error') {
    return <p className="text-destructive text-sm">Får ikke kontakt med API-et.</p>;
  }

  if (state.status === 'linked') {
    return (
      <p className="text-sm">
        Du er koblet som <span className="font-medium">{state.me.name}</span>.
      </p>
    );
  }

  return (
    <div className="flex max-w-md flex-col gap-6">
      <form onSubmit={handleCreate} className="flex items-end gap-2">
        <div className="flex flex-1 flex-col gap-1">
          <label htmlFor="staff-name" className="text-sm font-medium">
            Navn
          </label>
          <Input
            id="staff-name"
            value={name}
            onChange={(event) => setName(event.target.value)}
            required
          />
        </div>
        <Button type="submit">Opprett profil</Button>
      </form>

      {message && <p className="text-sm">{message}</p>}

      <ul className="flex flex-col gap-2">
        {state.staff.length === 0 && (
          <li className="text-muted-foreground text-sm">Ingen ansattprofiler ennå.</li>
        )}
        {state.staff.map((staffMember) => (
          <li
            key={staffMember.id}
            className="flex items-center justify-between rounded-lg border p-3"
          >
            <span>{staffMember.name}</span>
            {staffMember.auth0UserId ? (
              <span className="text-muted-foreground text-sm">Koblet</span>
            ) : (
              <Button
                type="button"
                size="sm"
                variant="outline"
                onClick={() => handleLink(staffMember.id)}
              >
                Koble til min konto
              </Button>
            )}
          </li>
        ))}
      </ul>
    </div>
  );
}
