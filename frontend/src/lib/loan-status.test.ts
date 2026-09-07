import { effectiveLoanStatus } from './loan-status';

describe('effectiveLoanStatus', () => {
  const now = new Date('2026-09-06T12:00:00Z');

  it('treats an Active loan past its due date as Overdue', () => {
    expect(
      effectiveLoanStatus({ status: 'Active', dueDate: '2026-09-01T00:00:00Z' }, now),
    ).toBe('Overdue');
  });

  it('leaves an Active loan alone before its due date', () => {
    expect(
      effectiveLoanStatus({ status: 'Active', dueDate: '2026-09-10T00:00:00Z' }, now),
    ).toBe('Active');
  });

  it('leaves an Active loan alone exactly on its due date', () => {
    expect(
      effectiveLoanStatus({ status: 'Active', dueDate: '2026-09-06T12:00:00Z' }, now),
    ).toBe('Active');
  });

  it('does not override a status the backend has already materialised', () => {
    expect(
      effectiveLoanStatus({ status: 'Overdue', dueDate: '2026-09-01T00:00:00Z' }, now),
    ).toBe('Overdue');
  });

  it('leaves closed statuses alone regardless of due date', () => {
    expect(
      effectiveLoanStatus({ status: 'Returned', dueDate: '2026-09-01T00:00:00Z' }, now),
    ).toBe('Returned');
    expect(effectiveLoanStatus({ status: 'Lost', dueDate: '2026-09-01T00:00:00Z' }, now)).toBe(
      'Lost',
    );
  });
});
