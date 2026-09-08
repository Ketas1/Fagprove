import { dateToIsoDate } from '@/lib/date';
import type { TimelineInterval } from '@/types/report';

/// The period choices on the reports page. `all` sends no dates at all, which
/// the API reads as the whole history - see docs/05-api.md.
export type PeriodPreset = 'last30' | 'last6m' | 'last12m' | 'all' | 'custom';

export type PeriodRange = {
  from: string | null;
  to: string | null;
};

export const periodOptions: readonly { value: PeriodPreset; label: string }[] = [
  { value: 'last30', label: 'Siste 30 dager' },
  { value: 'last6m', label: 'Siste 6 måneder' },
  { value: 'last12m', label: 'Siste 12 måneder' },
  { value: 'all', label: 'Hele historikken' },
  { value: 'custom', label: 'Egendefinert' },
];

export const intervalOptions: readonly { value: TimelineInterval; label: string }[] = [
  { value: 'Day', label: 'Dag' },
  { value: 'Week', label: 'Uke' },
  { value: 'Month', label: 'Måned' },
];

export function isPeriodPreset(value: string | undefined): value is PeriodPreset {
  return periodOptions.some((option) => option.value === value);
}

export function isTimelineInterval(value: string | undefined): value is TimelineInterval {
  return intervalOptions.some((option) => option.value === value);
}

/**
 * Adds (or subtracts) whole months without the end-of-month overflow
 * `Date.setMonth` has on its own - 31 August minus six months is 28 February,
 * not 3 March.
 */
function addMonths(date: Date, months: number): Date {
  const shifted = new Date(date.getFullYear(), date.getMonth() + months, 1);
  const lastDayOfMonth = new Date(shifted.getFullYear(), shifted.getMonth() + 1, 0).getDate();
  shifted.setDate(Math.min(date.getDate(), lastDayOfMonth));
  return shifted;
}

/**
 * Turns a preset into the `from`/`to` the API expects. `today` is passed in
 * rather than read from the clock so this stays testable, per the clock rule
 * in docs/07-testing.md.
 */
export function resolvePeriodRange(
  preset: PeriodPreset,
  today: Date,
  custom?: PeriodRange,
): PeriodRange {
  if (preset === 'all') {
    return { from: null, to: null };
  }

  if (preset === 'custom') {
    // An incomplete custom range would otherwise silently become "all time",
    // which is the opposite of what the staff member asked for.
    return custom?.from && custom.to ? custom : resolvePeriodRange('last30', today);
  }

  const to = dateToIsoDate(today);

  if (preset === 'last30') {
    const from = new Date(today.getFullYear(), today.getMonth(), today.getDate() - 29);
    return { from: dateToIsoDate(from), to };
  }

  return { from: dateToIsoDate(addMonths(today, preset === 'last6m' ? -6 : -12)), to };
}

/**
 * A day bucket over a year would be an unreadable chart (and the API refuses
 * more than 400 buckets outright), so each preset opens on the granularity
 * that actually fits it. The staff member can still switch.
 */
export function defaultIntervalFor(preset: PeriodPreset): TimelineInterval {
  return preset === 'last30' ? 'Day' : 'Month';
}

/** "01.03.2026 - 07.09.2026", or "Hele historikken" when unbounded. */
export function formatPeriodLabel(range: PeriodRange): string {
  if (!range.from || !range.to) {
    return 'Hele historikken';
  }

  return `${formatIsoDate(range.from)} - ${formatIsoDate(range.to)}`;
}

/** "2026-09-07" as "07.09.2026", without going through the timezone-shifting Date parser. */
export function formatIsoDate(value: string): string {
  const [year, month, day] = value.split('-');
  return `${day}.${month}.${year}`;
}

/**
 * How a bucket is labelled in the export and on the chart axis. Months read
 * as "2026-03" so they sort and group at a glance; days and weeks keep the
 * full date, a week being labelled by the Monday it starts on.
 */
export function formatBucketLabel(bucketStart: string, interval: TimelineInterval): string {
  return interval === 'Month' ? bucketStart.slice(0, 7) : bucketStart;
}
