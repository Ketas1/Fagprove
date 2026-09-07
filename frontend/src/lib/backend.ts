import { auth0 } from '@/lib/auth0';
import { resolveBackendUrl } from '@/lib/api';

/**
 * Server component data fetching, called directly from a page - not the
 * `/api/*` proxy route (`src/app/api/[...path]/route.ts`). That route exists
 * to keep the access token out of the browser (see
 * docs/adr/0015-proxied-backend-for-frontend.md); a server component never
 * exposes anything to the browser in the first place, so there is nothing to
 * gain from having the Next.js server call its own proxy route over HTTP.
 * Client-triggered mutations (the form modals) still go through the proxy,
 * since those genuinely originate in the browser.
 *
 * Only import this from a server component or another server-only module.
 */
export class BackendError extends Error {
  constructor(
    public readonly status: number,
    message: string,
  ) {
    super(message);
  }
}

export async function fetchBackend<T>(pathSegments: string[], search = ''): Promise<T> {
  const { token } = await auth0.getAccessToken();
  const url = resolveBackendUrl(pathSegments, search);

  const response = await fetch(url, {
    headers: { authorization: `Bearer ${token}` },
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new BackendError(response.status, `Backend returned ${response.status} for ${url.pathname}`);
  }

  return (await response.json()) as T;
}
