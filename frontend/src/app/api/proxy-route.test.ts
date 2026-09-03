/**
 * @jest-environment node
 *
 * Node environment because next/server's NextRequest needs the real Request/
 * Response globals, which jsdom does not provide. The mock below uses a
 * relative path rather than the `@/` alias deliberately - aliased jest.mock
 * targets fail to resolve from this directory specifically (a next/jest
 * quirk with per-file environment overrides), while a relative path works.
 */
import { NextRequest } from 'next/server';

jest.mock('../../lib/auth0', () => ({
  auth0: {
    getSession: jest.fn(),
    getAccessToken: jest.fn(),
  },
}));

import { auth0 } from '../../lib/auth0';
import { GET } from './[...path]/route';

const mockedAuth0 = jest.mocked(auth0);

function callGet(path: string[]) {
  const request = new NextRequest('http://localhost:3000/api/' + path.join('/'));

  return GET(request, { params: Promise.resolve({ path }) });
}

describe('GET /api/[...path]', () => {
  const originalFetch = global.fetch;

  afterEach(() => {
    jest.resetAllMocks();
    global.fetch = originalFetch;
  });

  it('returns 401 without forwarding anything when there is no session', async () => {
    mockedAuth0.getSession.mockResolvedValue(null);
    const fetchSpy = jest.fn();
    global.fetch = fetchSpy as unknown as typeof fetch;

    const response = await callGet(['health']);

    expect(response.status).toBe(401);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it('returns 401 when a token cannot be obtained', async () => {
    mockedAuth0.getSession.mockResolvedValue({ user: { sub: 'test-user' } } as never);
    mockedAuth0.getAccessToken.mockRejectedValue(new Error('no token'));
    const fetchSpy = jest.fn();
    global.fetch = fetchSpy as unknown as typeof fetch;

    const response = await callGet(['health']);

    expect(response.status).toBe(401);
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it('forwards to the backend with a bearer token and never forwards cookies', async () => {
    mockedAuth0.getSession.mockResolvedValue({ user: { sub: 'test-user' } } as never);
    mockedAuth0.getAccessToken.mockResolvedValue({ token: 'the-token', expiresAt: 0 } as never);

    const fetchSpy = jest.fn().mockResolvedValue(
      new Response(JSON.stringify({ status: 'ok', database: 'up' }), {
        status: 200,
        headers: { 'content-type': 'application/json' },
      }),
    );
    global.fetch = fetchSpy as unknown as typeof fetch;

    const response = await callGet(['health']);
    const body = await response.json();

    expect(response.status).toBe(200);
    expect(body).toEqual({ status: 'ok', database: 'up' });

    const [calledUrl, calledInit] = fetchSpy.mock.calls[0] as [URL, RequestInit];
    expect(calledUrl.toString()).toBe('http://localhost:5080/api/health');

    const forwardedHeaders = new Headers(calledInit.headers);
    expect(forwardedHeaders.get('authorization')).toBe('Bearer the-token');
    expect(forwardedHeaders.has('cookie')).toBe(false);
  });
});
