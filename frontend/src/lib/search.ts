/**
 * Normalizes a search query before matching: trims whitespace, lowercases,
 * and strips one leading "#" so a copied id/serial number like "#4554C09B"
 * matches without the user having to remove the "#" themselves.
 */
export function normalizeSearchQuery(query: string): string {
  const trimmed = query.trim().toLowerCase();
  return trimmed.startsWith('#') ? trimmed.slice(1) : trimmed;
}
