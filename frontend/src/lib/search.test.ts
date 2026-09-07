import { normalizeSearchQuery } from './search';

describe('normalizeSearchQuery', () => {
  it('trims and lowercases', () => {
    expect(normalizeSearchQuery('  Ola Nordmann  ')).toBe('ola nordmann');
  });

  it('strips one leading #', () => {
    expect(normalizeSearchQuery('#4554C09B')).toBe('4554c09b');
  });

  it('only strips a single leading #, not one appearing elsewhere', () => {
    expect(normalizeSearchQuery('a#b')).toBe('a#b');
  });

  it('leaves a query without # untouched apart from casing', () => {
    expect(normalizeSearchQuery('Ski')).toBe('ski');
  });
});
