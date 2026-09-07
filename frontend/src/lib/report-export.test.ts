import {
  buildReportSheets,
  buildReportTables,
  formatReportValue,
  reportFilename,
  toDisplayRows,
} from './report-export';
import type { ReportExportData } from './report-export';

const data: ReportExportData = {
  periodLabel: '01.03.2026 - 07.09.2026',
  generatedAt: new Date(2026, 8, 7, 14, 32),
  interval: 'Month',
  summary: {
    totalLoans: 412,
    returnedOnTime: 305,
    returnedLate: 37,
    notReturned: 6,
    stillActive: 64,
  },
  ageGroups: [
    { ageGroup: '3-7', figures: { totalLoans: 180, returnedOnTime: 140, returnedLate: 12, notReturned: 2, stillActive: 26 } },
    { ageGroup: '8-12', figures: { totalLoans: 52, returnedOnTime: 38, returnedLate: 5, notReturned: 1, stillActive: 8 } },
    { ageGroup: '13-18', figures: { totalLoans: 180, returnedOnTime: 127, returnedLate: 20, notReturned: 3, stillActive: 30 } },
  ],
  timeline: [
    { bucketStart: '2026-03-01', count: 61 },
    { bucketStart: '2026-04-01', count: 74 },
  ],
};

describe('buildReportTables', () => {
  it('produces the two reports plus the trend, in order, one per sheet', () => {
    expect(buildReportTables(data).map((table) => [table.sheetName, table.title])).toEqual([
      ['Sammendrag', '1. Utlån i perioden'],
      ['Aldersgrupper', '2. Utlån per aldersgruppe'],
      ['Utvikling', '3. Utvikling over tid'],
    ]);
  });

  it('keeps counts as numbers and shares as fractions, not preformatted text', () => {
    // The fraction is what lets Excel hold a real percentage the reader can
    // compute with, instead of the string "74 %".
    expect(buildReportTables(data)[0].rows).toEqual([
      ['Utlån totalt', 412, 1],
      ['Levert i tide', 305, 305 / 412],
      ['Levert for sent', 37, 37 / 412],
      ['Ikke levert', 6, 6 / 412],
      ['Fortsatt aktive', 64, 64 / 412],
    ]);
  });

  it('uses 0 rather than NaN when nothing was borrowed', () => {
    const empty: ReportExportData = {
      ...data,
      summary: { totalLoans: 0, returnedOnTime: 0, returnedLate: 0, notReturned: 0, stillActive: 0 },
    };

    expect(buildReportTables(empty)[0].rows[0]).toEqual(['Utlån totalt', 0, 0]);
  });

  it('adds a Sum row that reconciles the age groups against the summary', () => {
    const table = buildReportTables(data)[1];

    expect(table.hasTotalRow).toBe(true);
    expect(table.rows[table.rows.length - 1]).toEqual(['Sum', 412, 305, 37, 6, 64]);
    expect(table.rows[table.rows.length - 1][1]).toBe(data.summary.totalLoans);
  });

  it('carries the timeline period as an ISO string, never a Date', () => {
    const table = buildReportTables(data)[2];

    // A local-midnight Date here shifted 1 March to 28 February 23:00 once
    // Excel serialised it, so a March bucket displayed as 2026-02.
    expect(table.rows).toEqual([
      ['2026-03-01', 61],
      ['2026-04-01', 74],
    ]);
    // A total inside a time series breaks sorting and pivoting, and the
    // summary sheet already carries it.
    expect(table.hasTotalRow).toBe(false);
  });

  it('names the bucket size in the trend description', () => {
    expect(buildReportTables({ ...data, interval: 'Day' })[2].description).toBe('Antall utlån per dag.');
    expect(buildReportTables({ ...data, interval: 'Week' })[2].description).toBe('Antall utlån per uke.');
  });
});

describe('formatReportValue / toDisplayRows', () => {
  it('renders a fraction as whole percent for the PDF', () => {
    expect(formatReportValue(305 / 412, 'percent', 'Month')).toBe('74 %');
    expect(formatReportValue(1, 'percent', 'Month')).toBe('100 %');
  });

  it('shortens a month bucket, and keeps the full date otherwise', () => {
    expect(formatReportValue('2026-03-01', 'period', 'Month')).toBe('2026-03');
    expect(formatReportValue('2026-03-02', 'period', 'Day')).toBe('2026-03-02');
  });

  it('turns a whole table into the strings the PDF prints', () => {
    expect(toDisplayRows(buildReportTables(data)[0], 'Month')).toEqual([
      ['Utlån totalt', '412', '100 %'],
      ['Levert i tide', '305', '74 %'],
      ['Levert for sent', '37', '9 %'],
      ['Ikke levert', '6', '1 %'],
      ['Fortsatt aktive', '64', '16 %'],
    ]);
  });
});

describe('buildReportSheets', () => {
  const sheets = buildReportSheets(data);

  it('writes one sheet per report, named for it', () => {
    expect(sheets.map((sheet) => sheet.sheet)).toEqual(['Sammendrag', 'Aldersgrupper', 'Utvikling']);
  });

  /**
   * The bug this whole format change exists to fix: as CSV, Excel read the age
   * group "3-7" as 3. juli and "8-12" as 8. desember, while "13-18" stayed
   * text because there is no month 18 - so the column was both wrong and
   * inconsistent. Typing the cell as String settles it in the file itself.
   */
  it('types every age-group label as text so Excel cannot read it as a date', () => {
    const ageGroupSheet = sheets[1];
    const labelCells = ageGroupSheet.data.slice(1).map((row) => row[0]);

    expect(labelCells.map((cell) => cell.value)).toEqual(['3-7', '8-12', '13-18', 'Sum']);
    for (const cell of labelCells) {
      expect(cell.type).toBe(String);
    }
  });

  it('types counts as numbers and shares as percent-formatted numbers', () => {
    const firstDataRow = sheets[0].data[5];

    expect(firstDataRow[1]).toMatchObject({ value: 412, type: Number, format: '0' });
    expect(firstDataRow[2]).toMatchObject({ value: 1, type: Number, format: '0 %' });
  });

  it('types the timeline period as a UTC-midnight date with a month format', () => {
    const firstDataRow = sheets[2].data[1];

    // UTC midnight, so Excel's day serial is a whole number. A local-midnight
    // date serialises with a time fraction and lands on the previous day in
    // any timezone ahead of UTC - which showed 2026-02 for a March bucket.
    expect(firstDataRow[0]).toMatchObject({
      value: new Date(Date.UTC(2026, 2, 1)),
      type: Date,
      format: 'yyyy-mm',
    });
    expect((firstDataRow[0].value as Date).getUTCHours()).toBe(0);
    expect(firstDataRow[1]).toMatchObject({ value: 61, type: Number });
  });

  it('switches the date format when the buckets are days', () => {
    const daySheets = buildReportSheets({ ...data, interval: 'Day' });

    expect(daySheets[2].data[1][0]).toMatchObject({ format: 'yyyy-mm-dd' });
  });

  it('puts the period and generation time on the first sheet only', () => {
    expect(sheets[0].data[1][0].value).toBe('Periode');
    expect(sheets[0].data[1][1].value).toBe('01.03.2026 - 07.09.2026');
    expect(sheets[0].data[2][1].value).toBe('07.09.2026 14:32');

    // The other two start at the header row, so they stay rectangular and can
    // be sorted, filtered and pivoted.
    expect(sheets[1].data[0].map((cell) => cell.value)).toEqual([
      'Aldersgruppe',
      'Utlån totalt',
      'Levert i tide',
      'Levert for sent',
      'Ikke levert',
      'Fortsatt aktive',
    ]);
  });

  it('emphasises the Sum row and freezes the header', () => {
    const ageGroupSheet = sheets[1];

    expect(ageGroupSheet.stickyRowsCount).toBe(1);
    expect(sheets[0].stickyRowsCount).toBe(5);
    for (const cell of ageGroupSheet.data[ageGroupSheet.data.length - 1]) {
      expect(cell.fontWeight).toBe('bold');
    }
  });

  it('gives every column an explicit width', () => {
    for (const sheet of sheets) {
      expect(sheet.columns.length).toBeGreaterThan(0);
      for (const column of sheet.columns) {
        expect(column.width).toBeGreaterThan(0);
      }
    }
  });

  it('never contains a name, an id or a single loan', () => {
    // The export is aggregate-only by construction - see
    // docs/09-lover-og-regler.md. A uuid here would mean an entity leaked into
    // what gets handed to the municipality.
    const everything = JSON.stringify(sheets);

    expect(everything).not.toMatch(/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i);
  });
});

describe('reportFilename', () => {
  it('names the file after the day it was generated', () => {
    expect(reportFilename(new Date(2026, 8, 7), 'xlsx')).toBe('rapport-sport-for-alle-2026-09-07.xlsx');
    expect(reportFilename(new Date(2026, 8, 7), 'pdf')).toBe('rapport-sport-for-alle-2026-09-07.pdf');
  });
});
