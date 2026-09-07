/**
 * Converts a local `Date` to a "yyyy-MM-dd" string using its local
 * year/month/day - never `.toISOString()`, which converts to UTC first and
 * can shift the date by a day depending on the browser's timezone offset.
 */
export function dateToIsoDate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${year}-${month}-${day}`;
}

/**
 * Parses a "yyyy-MM-dd" string into a local `Date` at midnight local time -
 * the counterpart to {@link dateToIsoDate}. `new Date("yyyy-MM-dd")` parses
 * as UTC midnight instead, which is the same bug in reverse. Returns `null`
 * for an empty or malformed string.
 */
export function isoDateToDate(value: string): Date | null {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value);
  if (!match) return null;

  const [, year, month, day] = match;
  const date = new Date(Number(year), Number(month) - 1, Number(day));
  return Number.isNaN(date.getTime()) ? null : date;
}
