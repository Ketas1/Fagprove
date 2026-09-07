import { dateToIsoDate, isoDateToDate } from './date';

describe('dateToIsoDate', () => {
  it('formats using local year/month/day, zero-padded', () => {
    expect(dateToIsoDate(new Date(2026, 0, 5))).toBe('2026-01-05');
  });
});

describe('isoDateToDate', () => {
  it('parses a valid date string into a local date at midnight', () => {
    const date = isoDateToDate('2018-08-12');
    expect(date).not.toBeNull();
    expect(date!.getFullYear()).toBe(2018);
    expect(date!.getMonth()).toBe(7);
    expect(date!.getDate()).toBe(12);
  });

  it('returns null for an empty string', () => {
    expect(isoDateToDate('')).toBeNull();
  });

  it('returns null for a malformed string', () => {
    expect(isoDateToDate('12-08-2018')).toBeNull();
  });

  it('round-trips through dateToIsoDate', () => {
    expect(dateToIsoDate(isoDateToDate('2018-08-12')!)).toBe('2018-08-12');
  });
});
