'use client';

import { useEffect, useState } from 'react';
import type { Health } from '@/types/health';
import { HealthStatus } from '@/components/HealthStatus';
import { Skeleton } from '@/components/ui/skeleton';

type State = { status: 'loading' } | { status: 'loaded'; health: Health | null };

/**
 * Fetches through the same-origin proxy (never the backend directly) - the
 * browser's own session cookie is what lets the proxy attach a token
 * server-side. See docs/adr/0015-proxied-backend-for-frontend.md.
 *
 * "Still loading" and "the API could not be reached" are deliberately
 * different states - health is null in both, but only the second one is a
 * real error worth showing.
 */
export function StatusWidget() {
  const [state, setState] = useState<State>({ status: 'loading' });

  useEffect(() => {
    const controller = new AbortController();

    fetch('/api/health', { cache: 'no-store', signal: controller.signal })
      .then((response) => (response.ok ? (response.json() as Promise<Health>) : null))
      .then((result) => {
        setState({ status: 'loaded', health: result });
      })
      .catch((error: unknown) => {
        // An aborted request (React Strict Mode's mount-cleanup-remount in
        // development, or unmounting before the fetch settles) is not a
        // real failure - only a genuinely failed fetch should show the
        // error state.
        if (error instanceof DOMException && error.name === 'AbortError') {
          return;
        }

        setState({ status: 'loaded', health: null });
      });

    return () => {
      controller.abort();
    };
  }, []);

  if (state.status === 'loading') {
    return <Skeleton className="h-24 w-full max-w-sm" />;
  }

  return <HealthStatus health={state.health} />;
}
