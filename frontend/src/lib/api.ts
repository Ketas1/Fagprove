const API_BASE_URL = process.env.API_BASE_URL ?? 'http://localhost:5080';

/**
 * Resolves a path forwarded through the proxy (`src/app/api/[...path]/route.ts`)
 * to a URL on the real backend, and refuses to resolve it anywhere else.
 *
 * Built with the `URL` constructor rather than string concatenation. The
 * hardcoded `api/` prefix already keeps the resolved reference from ever
 * starting with `//` or a scheme - the two ways `new URL(relative, base)`
 * can otherwise ignore `base` entirely and resolve to a different origin -
 * but the origin check stays as defence in depth against a future change to
 * this function accidentally removing that guarantee. See
 * docs/adr/0015-proxied-backend-for-frontend.md.
 */
export function resolveBackendUrl(pathSegments: string[], search: string): URL {
  const base = new URL(API_BASE_URL);
  const target = new URL(`api/${pathSegments.join('/')}`, API_BASE_URL);
  target.search = search;

  if (target.origin !== base.origin) {
    throw new Error(`Resolved backend URL left the trusted origin: ${target.origin}`);
  }

  return target;
}
