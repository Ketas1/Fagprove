/**
 * Builds an absolute URL to a backend endpoint.
 *
 * The base URL comes from NEXT_PUBLIC_API_BASE_URL so the same code works when
 * the API runs locally and when it runs elsewhere. Leading and trailing slashes
 * are normalised so callers can pass either form.
 */
export function apiUrl(path: string): string {
  const base = (process.env.NEXT_PUBLIC_API_BASE_URL ?? 'http://localhost:5080').replace(/\/+$/, '');
  const suffix = path.startsWith('/') ? path : `/${path}`;

  return `${base}${suffix}`;
}
