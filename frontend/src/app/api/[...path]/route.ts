import { NextRequest, NextResponse } from 'next/server';
import { auth0 } from '@/lib/auth0';
import { resolveBackendUrl } from '@/lib/api';

/**
 * Forwards every request under /api/* to the real backend, attaching the
 * access token server-side. The browser only ever talks to this same-origin
 * route - it never sees the backend's address or a token. See
 * docs/adr/0015-proxied-backend-for-frontend.md.
 *
 * Two things here are not negotiable, agreed on before this was built:
 *
 * 1. Outgoing headers are built from scratch, never forwarded from the
 *    incoming request. `Cookie` holds this app's own encrypted session and
 *    must never reach the backend.
 * 2. The backend URL is resolved with `resolveBackendUrl`, never by
 *    concatenating the incoming path into a string.
 *
 * `dynamic = 'force-dynamic'` matters more here than it looks: without it,
 * Next.js may treat this GET handler as static and cache its very first
 * response (a 401, before login) and keep serving that same cached response
 * forever afterwards, regardless of session state - which looks exactly
 * like "authentication never works," even once it actually does.
 */
export const dynamic = 'force-dynamic';

async function proxy(request: NextRequest, context: { params: Promise<{ path: string[] }> }) {
  const session = await auth0.getSession();

  if (!session) {
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  let token: string;

  try {
    ({ token } = await auth0.getAccessToken());
  } catch (error) {
    // Not personal data - just the SDK's own error about why a token
    // couldn't be obtained, which is otherwise invisible to whoever is
    // debugging a 401 from this route.
    console.error('[api proxy] could not obtain an access token:', error);
    return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
  }

  const { path } = await context.params;
  const targetUrl = resolveBackendUrl(path, request.nextUrl.search);

  const outgoingHeaders = new Headers();
  const contentType = request.headers.get('content-type');

  if (contentType) {
    outgoingHeaders.set('content-type', contentType);
  }

  outgoingHeaders.set('authorization', `Bearer ${token}`);

  const hasBody = !['GET', 'HEAD'].includes(request.method);

  let backendResponse: Response;

  try {
    backendResponse = await fetch(targetUrl, {
      method: request.method,
      headers: outgoingHeaders,
      body: hasBody ? request.body : undefined,
      // Required by undici when streaming a request body; there is no
      // corresponding field on the RequestInit type yet.
      // @ts-expect-error - see comment above
      duplex: hasBody ? 'half' : undefined,
    });
  } catch (error) {
    console.error('[api proxy] could not reach the backend at', targetUrl.origin, error);
    return NextResponse.json({ error: 'Bad Gateway' }, { status: 502 });
  }

  const responseHeaders = new Headers();
  const responseContentType = backendResponse.headers.get('content-type');

  if (responseContentType) {
    responseHeaders.set('content-type', responseContentType);
  }

  return new NextResponse(backendResponse.body, {
    status: backendResponse.status,
    headers: responseHeaders,
  });
}

export {
  proxy as GET,
  proxy as POST,
  proxy as PUT,
  proxy as PATCH,
  proxy as DELETE,
};
