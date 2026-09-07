/// Shapes returned by GET /api/reports/*. See docs/05-api.md and the
/// "Rapportering" section of docs/03-domenemodell.md.
///
/// Every field is a count. Nothing in these responses identifies a borrower,
/// a guardian or a single loan - see docs/09-lover-og-regler.md.

/**
 * The five figures both reports share. Because all five count loans whose
 * `startedAt` falls in the period, and there are exactly four loan statuses,
 * `returnedOnTime + returnedLate + notReturned + stillActive === totalLoans`.
 */
export type LoanFigures = {
  totalLoans: number;
  returnedOnTime: number;
  returnedLate: number;
  notReturned: number;
  stillActive: number;
};

/// Report 1 of 2. `from`/`to` are null when the whole history was requested.
export type LoanSummaryReport = {
  from: string | null;
  to: string | null;
  figures: LoanFigures;
};

export type AgeGroupFigures = {
  ageGroup: string;
  figures: LoanFigures;
};

/// Report 2 of 2 - the same figures split by age at the time of the loan.
export type AgeGroupReport = {
  from: string | null;
  to: string | null;
  groups: AgeGroupFigures[];
};

export type TimelineInterval = 'Day' | 'Week' | 'Month';

export type TimelineBucket = {
  /** First day of the bucket - the Monday for a week, the 1st for a month. */
  bucketStart: string;
  count: number;
};

/// A breakdown of `totalLoans` over time for the trend chart, not a third report.
export type LoanTimelineReport = {
  from: string | null;
  to: string | null;
  interval: TimelineInterval;
  buckets: TimelineBucket[];
};
