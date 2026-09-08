import {
  defaultIntervalFor,
  formatBucketLabel,
  formatIsoDate,
  formatPeriodLabel,
  isPeriodPreset,
  isTimelineInterval,
  resolvePeriodRange,
} from './report-period';

describe('resolvePeriodRange', () => {
  const today = new Date(2026, 8, 7); // 7 September 2026, local time.

  it('sends no dates at all for the whole history', () => {
    expect(resolvePeriodRange('all', today)).toEqual({ from: null, to: null });
  });

  it('counts the last 30 days inclusive of today', () => {
    expect(resolvePeriodRange('last30', today)).toEqual({ from: '2026-08-09', to: '2026-09-07' });
  });

  it('goes back six whole months', () => {
    expect(resolvePeriodRange('last6m', today)).toEqual({ from: '2026-03-07', to: '2026-09-07' });
  });

  it('goes back twelve whole months', () => {
    expect(resolvePeriodRange('last12m', today)).toEqual({ from: '2025-09-07', to: '2026-09-07' });
  });

  it('clamps to the last day of a shorter month instead of overflowing', () => {
    // 31 August minus six months is 28 February, not 3 March.
    expect(resolvePeriodRange('last6m', new Date(2026, 7, 31)).from).toBe('2026-02-28');
  });

  it('passes a complete custom range through unchanged', () => {
    const custom = { from: '2026-01-01', to: '2026-01-31' };
    expect(resolvePeriodRange('custom', today, custom)).toEqual(custom);
  });

  it('falls back to the last 30 days when a custom range is half filled in', () => {
    // Treating a half-filled range as "all time" would quietly answer a
    // different question than the one that was asked.
    expect(resolvePeriodRange('custom', today, { from: '2026-01-01', to: null })).toEqual({
      from: '2026-08-09',
      to: '2026-09-07',
    });
    expect(resolvePeriodRange('custom', today, undefined)).toEqual({
      from: '2026-08-09',
      to: '2026-09-07',
    });
  });
});

describe('defaultIntervalFor', () => {
  it('opens a short period on days and a long one on months', () => {
    expect(defaultIntervalFor('last30')).toBe('Day');
    expect(defaultIntervalFor('last6m')).toBe('Month');
    expect(defaultIntervalFor('last12m')).toBe('Month');
    expect(defaultIntervalFor('all')).toBe('Month');
  });
});

describe('formatPeriodLabel', () => {
  it('formats a bounded period in Norwegian date order', () => {
    expect(formatPeriodLabel({ from: '2026-03-01', to: '2026-09-07' })).toBe('01.03.2026 - 07.09.2026');
  });

  it.each([
    [{ from: null, to: null }],
    [{ from: '2026-03-01', to: null }],
    [{ from: null, to: '2026-09-07' }],
  ])('calls an unbounded period the whole history (%o)', (range) => {
    expect(formatPeriodLabel(range)).toBe('Hele historikken');
  });
});

describe('formatIsoDate', () => {
  it('reorders without going through the timezone-shifting Date parser', () => {
    expect(formatIsoDate('2026-01-05')).toBe('05.01.2026');
  });
});

describe('formatBucketLabel', () => {
  it('shortens a month bucket to year and month', () => {
    expect(formatBucketLabel('2026-03-01', 'Month')).toBe('2026-03');
  });

  it('keeps the full date for a day or week bucket', () => {
    expect(formatBucketLabel('2026-03-02', 'Day')).toBe('2026-03-02');
    expect(formatBucketLabel('2026-03-02', 'Week')).toBe('2026-03-02');
  });
});

describe('guards', () => {
  it('accepts only known presets and intervals', () => {
    expect(isPeriodPreset('last30')).toBe(true);
    expect(isPeriodPreset('siste-uke')).toBe(false);
    expect(isPeriodPreset(undefined)).toBe(false);
    expect(isTimelineInterval('Month')).toBe(true);
    expect(isTimelineInterval('month')).toBe(false);
  });
});
