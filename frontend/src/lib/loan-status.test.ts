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

  it('leaves an Active loan alone when due earlier the same day, not just the same instant', () => {
    // The reported bug: a loan due "today" at any earlier time-of-day
    // (00:00, in this case) must not read as Overdue while it is still
    // "today" - only the calendar date matters, not the stored time.
    expect(
      effectiveLoanStatus({ status: 'Active', dueDate: '2026-09-06T00:00:00Z' }, now),
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
