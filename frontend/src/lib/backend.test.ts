/**
 * @jest-environment node
 *
 * Node environment because `fetchBackend` is server-only code and the assertions
 * below use the real `Response` global, which jsdom does not provide. Mock
 * targets use relative paths rather than the `@/` alias, matching
 * `app/api/proxy-route.test.ts`.
 */
// `@auth0/nextjs-auth0/errors` is ESM-only, and Jest loads it as CommonJS -
// importing it for real fails the whole suite. A factory mock stops Jest
// reaching the real module at all, and because `backend.ts` resolves to this
// same mock, the `instanceof` check in the code under test still works. The
// shape mirrors the real class, confirmed against
// node_modules/@auth0/nextjs-auth0/dist/errors/oauth-errors.js.
jest.mock('@auth0/nextjs-auth0/errors', () => ({
  AccessTokenError: class AccessTokenError extends Error {
    constructor(
      public readonly code: string,
      message: string,
    ) {
      super(message);
      this.name = 'AccessTokenError';
    }
  },
  AccessTokenErrorCode: {
    MISSING_SESSION: 'missing_session',
    MISSING_REFRESH_TOKEN: 'missing_refresh_token',
    FAILED_TO_REFRESH_TOKEN: 'failed_to_refresh_token',
    SESSION_EXPIRED: 'session_expired',
  },
}));

import { AccessTokenError, AccessTokenErrorCode } from '@auth0/nextjs-auth0/errors';

jest.mock('./auth0', () => ({
  auth0: {
    getSession: jest.fn(),
    getAccessToken: jest.fn(),
  },
}));

// Stands in for the real `redirect`, which signals a redirect by throwing a
// special error rather than returning - so the test has to throw too, or the
// code under test would carry on with no token.
jest.mock('next/navigation', () => ({
  redirect: jest.fn((destination: string) => {
    throw new Error(`NEXT_REDIRECT:${destination}`);
  }),
}));

import { redirect } from 'next/navigation';
import { auth0 } from './auth0';
import { BackendError, fetchBackend } from './backend';

const mockedAuth0 = jest.mocked(auth0);
const mockedRedirect = jest.mocked(redirect);

describe('fetchBackend', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    jest.clearAllMocks();
    global.fetch = originalFetch;
  });

  it('attaches the access token and returns the parsed body', async () => {
    mockedAuth0.getAccessToken.mockResolvedValue({ token: 'a-token' } as never);
    const fetchSpy = jest.fn().mockResolvedValue(Response.json({ ok: true }));
    global.fetch = fetchSpy as unknown as typeof fetch;

    await expect(fetchBackend(['health'])).resolves.toEqual({ ok: true });

    const [url, init] = fetchSpy.mock.calls[0];
    expect(url.pathname).toBe('/api/health');
    expect(init.headers.authorization).toBe('Bearer a-token');
  });

  /**
   * The bug this guard exists for: the session cookie outlives the access
   * token, so `app/dashboard/layout.tsx` sees a valid session and renders,
   * and the dead token only surfaces here - as a 500 mid-render rather than
   * a trip back to the login page. See docs/06-autentisering.md.
   */
  it('sends the user to log in again when the token cannot be renewed', async () => {
    mockedAuth0.getAccessToken.mockRejectedValue(
      new AccessTokenError(
        AccessTokenErrorCode.MISSING_REFRESH_TOKEN,
        'The access token has expired and a refresh token was not provided.',
      ),
    );
    const fetchSpy = jest.fn();
    global.fetch = fetchSpy as unknown as typeof fetch;

    await expect(fetchBackend(['loans'])).rejects.toThrow('NEXT_REDIRECT:/auth/login');

    expect(mockedRedirect).toHaveBeenCalledWith('/auth/login');
    // Nothing should reach the backend without a token.
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it.each([
    AccessTokenErrorCode.FAILED_TO_REFRESH_TOKEN,
    AccessTokenErrorCode.MISSING_SESSION,
  ])('also signs in again for %s', async (code) => {
    mockedAuth0.getAccessToken.mockRejectedValue(new AccessTokenError(code, 'nope'));

    await expect(fetchBackend(['loans'])).rejects.toThrow('NEXT_REDIRECT:/auth/login');
  });

  /**
   * A misconfigured tenant or a missing environment variable is a real fault.
   * Disguising it as a login prompt would send the developer hunting for an
   * auth problem that is not there.
   */
  it('rethrows anything that is not an access-token failure', async () => {
    mockedAuth0.getAccessToken.mockRejectedValue(new TypeError('AUTH0_DOMAIN is not defined'));

    await expect(fetchBackend(['loans'])).rejects.toThrow('AUTH0_DOMAIN is not defined');
    expect(mockedRedirect).not.toHaveBeenCalled();
  });

  it('turns a non-ok backend response into a BackendError carrying the status', async () => {
    mockedAuth0.getAccessToken.mockResolvedValue({ token: 'a-token' } as never);
    global.fetch = jest
      .fn()
      .mockResolvedValue(new Response('nope', { status: 503 })) as unknown as typeof fetch;

    await expect(fetchBackend(['loans'])).rejects.toBeInstanceOf(BackendError);
    // Not a redirect: the user is signed in, the backend is simply unwell.
    expect(mockedRedirect).not.toHaveBeenCalled();
  });
});
