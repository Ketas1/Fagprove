import { redirect } from 'next/navigation';
import { AccessTokenError } from '@auth0/nextjs-auth0/errors';
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
  const token = await getAccessTokenOrSignIn();
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

/**
 * The session cookie and the access token inside it expire independently.
 * `auth0.getSession()` only decrypts the cookie, so the guard in
 * `app/dashboard/layout.tsx` still passes while the token behind it is long
 * dead - and without a refresh token there is nothing to renew it with. The
 * result used to be an `AccessTokenError` thrown mid-render: a 500 in
 * development and a "Noe gikk galt" page in production, when the honest
 * answer is simply that the user needs to sign in again.
 *
 * Only `AccessTokenError` is treated this way. Anything else - a
 * misconfigured tenant, a missing environment variable - is a real fault and
 * is left to surface, rather than being disguised as a login prompt.
 *
 * Known limitation: if Auth0 hands back a token that is unusable *immediately*
 * after a successful login, this redirects into a loop. That requires a
 * fundamentally broken tenant configuration - a freshly issued token is valid
 * by definition - but it is the failure mode to look for if the login page
 * ever starts repeating itself. See docs/06-autentisering.md.
 */
async function getAccessTokenOrSignIn(): Promise<string> {
  try {
    const { token } = await auth0.getAccessToken();
    return token;
  } catch (error) {
    if (!(error instanceof AccessTokenError)) {
      throw error;
    }

    // The SDK's own reason code (for example `missing_refresh_token`), which
    // is otherwise invisible to whoever is debugging an unexpected trip back
    // to the login page. Not personal data.
    console.error('[backend] no usable access token, signing in again:', error.code);

    redirect('/auth/login');
  }
}
