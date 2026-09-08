import { ageGroup, calculateAge } from './age';

describe('calculateAge', () => {
  const at = new Date('2026-09-06T00:00:00Z');

  it('counts a full year for a birthday already passed this year', () => {
    expect(calculateAge('2018-08-12', at)).toBe(8);
  });

  it('does not count the year yet for a birthday still ahead this year', () => {
    expect(calculateAge('2018-12-01', at)).toBe(7);
  });

  it('counts the birthday itself as turned', () => {
    expect(calculateAge('2018-09-06', at)).toBe(8);
  });
});

describe('ageGroup', () => {
  it.each([
    [3, '3-7'],
    [7, '3-7'],
    [8, '8-12'],
    [12, '8-12'],
    [13, '13-18'],
    [18, '13-18'],
  ] as const)('maps age %i to group %s', (age, expected) => {
    expect(ageGroup(age)).toBe(expected);
  });

  it('returns null outside the scheme range', () => {
    expect(ageGroup(2)).toBeNull();
    expect(ageGroup(19)).toBeNull();
  });
});
