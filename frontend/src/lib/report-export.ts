import { dateToIsoDate } from '@/lib/date';
import { formatBucketLabel, formatIsoDate } from '@/lib/report-period';
import type { AgeGroupFigures, LoanFigures, TimelineBucket, TimelineInterval } from '@/types/report';

/**
 * Everything the PDF and the Excel file are built from. Only aggregated counts
 * - no names, no ids, no single loans. See the export note in
 * docs/01-losningsbeskrivelse.md and the reporting basis in
 * docs/09-lover-og-regler.md.
 */
export type ReportExportData = {
  periodLabel: string;
  generatedAt: Date;
  interval: TimelineInterval;
  summary: LoanFigures;
  ageGroups: AgeGroupFigures[];
  timeline: TimelineBucket[];
};

/**
 * What a column holds. This is the whole reason the Excel file behaves: a
 * `label` cell is written as text even when it looks like a date, so Excel
 * cannot turn the age group "3-7" into 3. juli. A CSV has nowhere to put this
 * information, which is why it was dropped - see ADR-0023.
 */
export type ColumnKind = 'label' | 'number' | 'percent' | 'period';

export type ReportColumn = {
  header: string;
  kind: ColumnKind;
  /** Column width in characters, for the Excel sheet. */
  width: number;
};

/**
 * Deliberately no `Date` here. A period is carried as its "yyyy-MM-dd" string
 * and only turned into a date inside the Excel cell, as UTC midnight. Putting
 * a local-midnight `Date` in the model shifted 1 March to 28 February 23:00
 * once it was serialised, so a month bucket rendered as the previous month -
 * the same timezone trap `lib/date.ts` warns about.
 */
export type ReportValue = string | number;

export type ReportTable = {
  sheetName: string;
  title: string;
  description: string;
  columns: ReportColumn[];
  rows: ReportValue[][];
  /** True when the last row is a total and should be emphasised. */
  hasTotalRow: boolean;
};

const intervalNouns: Record<TimelineInterval, string> = {
  Day: 'dag',
  Week: 'uke',
  Month: 'måned',
};

/** A share of the total, as a fraction. Excel formats it; the PDF rounds it. */
function fraction(value: number, total: number): number {
  return total === 0 ? 0 : value / total;
}

function formatTimestamp(date: Date): string {
  const time = `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  return `${formatIsoDate(dateToIsoDate(date))} ${time}`;
}

function sumFigures(all: LoanFigures[]): LoanFigures {
  return all.reduce(
    (running, figures) => ({
      totalLoans: running.totalLoans + figures.totalLoans,
      returnedOnTime: running.returnedOnTime + figures.returnedOnTime,
      returnedLate: running.returnedLate + figures.returnedLate,
      notReturned: running.notReturned + figures.notReturned,
      stillActive: running.stillActive + figures.stillActive,
    }),
    { totalLoans: 0, returnedOnTime: 0, returnedLate: 0, notReturned: 0, stillActive: 0 },
  );
}

/**
 * The single typed source both export formats render from, so a number cannot
 * appear in the PDF and differ in the Excel file.
 */
export function buildReportTables(data: ReportExportData): ReportTable[] {
  const { summary, ageGroups, timeline, interval } = data;
  const total = summary.totalLoans;

  const figureRow = (label: string, figures: LoanFigures): ReportValue[] => [
    label,
    figures.totalLoans,
    figures.returnedOnTime,
    figures.returnedLate,
    figures.notReturned,
    figures.stillActive,
  ];

  return [
    {
      sheetName: 'Sammendrag',
      title: '1. Utlån i perioden',
      description: 'Alle utlån som ble registrert i perioden.',
      columns: [
        { header: 'Rad', kind: 'label', width: 28 },
        { header: 'Antall', kind: 'number', width: 12 },
        { header: 'Andel', kind: 'percent', width: 10 },
      ],
      rows: [
        ['Utlån totalt', summary.totalLoans, fraction(summary.totalLoans, total)],
        ['Levert i tide', summary.returnedOnTime, fraction(summary.returnedOnTime, total)],
        ['Levert for sent', summary.returnedLate, fraction(summary.returnedLate, total)],
        ['Ikke levert', summary.notReturned, fraction(summary.notReturned, total)],
        ['Fortsatt aktive', summary.stillActive, fraction(summary.stillActive, total)],
      ],
      hasTotalRow: false,
    },
    {
      sheetName: 'Aldersgrupper',
      title: '2. Utlån per aldersgruppe',
      description: 'Alder er regnet på utlånstidspunktet, ikke i dag.',
      columns: [
        { header: 'Aldersgruppe', kind: 'label', width: 16 },
        { header: 'Utlån totalt', kind: 'number', width: 14 },
        { header: 'Levert i tide', kind: 'number', width: 14 },
        { header: 'Levert for sent', kind: 'number', width: 16 },
        { header: 'Ikke levert', kind: 'number', width: 14 },
        { header: 'Fortsatt aktive', kind: 'number', width: 16 },
      ],
      rows: [
        ...ageGroups.map((group) => figureRow(group.ageGroup, group.figures)),
        figureRow('Sum', sumFigures(ageGroups.map((group) => group.figures))),
      ],
      hasTotalRow: true,
    },
    {
      sheetName: 'Utvikling',
      title: '3. Utvikling over tid',
      description: `Antall utlån per ${intervalNouns[interval]}.`,
      columns: [
        { header: 'Periode', kind: 'period', width: 14 },
        { header: 'Utlån', kind: 'number', width: 12 },
      ],
      // The Excel cell turns this into a real date, so the sheet can sort,
      // filter and chart it as a time axis. No Sum row here: a total inside a
      // time series breaks sorting and pivoting, and the summary sheet
      // already carries it.
      rows: timeline.map((bucket) => [bucket.bucketStart, bucket.count]),
      hasTotalRow: false,
    },
  ];
}

/** How a value reads in the PDF, where everything is text anyway. */
export function formatReportValue(
  value: ReportValue,
  kind: ColumnKind,
  interval: TimelineInterval,
): string {
  if (kind === 'percent') {
    return `${Math.round(Number(value) * 100)} %`;
  }

  if (kind === 'period') {
    return formatBucketLabel(String(value), interval);
  }

  return String(value);
}

export function toDisplayRows(table: ReportTable, interval: TimelineInterval): string[][] {
  return table.rows.map((row) =>
    row.map((value, index) => formatReportValue(value, table.columns[index].kind, interval)),
  );
}

export function reportFilename(generatedAt: Date, extension: 'xlsx' | 'pdf'): string {
  return `rapport-sport-for-alle-${dateToIsoDate(generatedAt)}.${extension}`;
}

/** Excel number formats per column kind. `0 %` turns 0.74 into "74 %". */
const excelFormats: Record<ColumnKind, string | undefined> = {
  label: undefined,
  number: '0',
  percent: '0 %',
  period: undefined,
};

/** Excel stores a date as a day number; any time-of-day shows up as a fraction. */
function isoToUtcDate(iso: string): Date {
  const [year, month, day] = iso.split('-').map(Number);
  return new Date(Date.UTC(year, month - 1, day));
}

type ExcelCell = {
  value: ReportValue | Date;
  type: StringConstructor | NumberConstructor | DateConstructor;
  format?: string;
  fontWeight?: 'bold';
  align?: 'left' | 'center' | 'right';
  backgroundColor?: string;
  color?: string;
};

function excelCell(value: ReportValue, column: ReportColumn, interval: TimelineInterval, bold: boolean): ExcelCell {
  const base = bold ? { fontWeight: 'bold' as const } : {};

  if (column.kind === 'period') {
    return {
      ...base,
      value: isoToUtcDate(String(value)),
      type: Date,
      format: interval === 'Month' ? 'yyyy-mm' : 'yyyy-mm-dd',
    };
  }

  // A total row's first cell is the word "Sum" even in a numeric table, so the
  // type follows the value rather than the column when they disagree.
  if (column.kind === 'label' || typeof value === 'string') {
    return { ...base, value: String(value), type: String };
  }

  return { ...base, value: Number(value), type: Number, format: excelFormats[column.kind], align: 'right' };
}

/**
 * One sheet per table. Kept separate from the file writing so the typing of
 * every cell can be unit tested without running the library - the point of the
 * whole change is that "3-7" stays text, and that is worth an assertion.
 */
export function buildReportSheets(data: ReportExportData) {
  return buildReportTables(data).map((table, index) => {
    const headerRow = table.columns.map((column) => ({
      value: column.header,
      type: String as StringConstructor,
      fontWeight: 'bold' as const,
      backgroundColor: '#1F6E45',
      color: '#FFFFFF',
      align: column.kind === 'label' ? ('left' as const) : ('right' as const),
    }));

    const bodyRows = table.rows.map((row, rowIndex) => {
      const bold = table.hasTotalRow && rowIndex === table.rows.length - 1;
      return row.map((value, columnIndex) => excelCell(value, table.columns[columnIndex], data.interval, bold));
    });

    // The first sheet carries the report's identity: which period the numbers
    // cover, and when they were pulled. The other two start at the header row
    // so they stay rectangular and can be sorted, filtered and pivoted.
    const preamble =
      index === 0
        ? [
            [{ value: 'Rapport - Sport For Alle', type: String as StringConstructor, fontWeight: 'bold' as const }],
            [
              { value: 'Periode', type: String as StringConstructor, fontWeight: 'bold' as const },
              { value: data.periodLabel, type: String as StringConstructor },
            ],
            [
              { value: 'Generert', type: String as StringConstructor, fontWeight: 'bold' as const },
              { value: formatTimestamp(data.generatedAt), type: String as StringConstructor },
            ],
            [],
          ]
        : [];

    return {
      sheet: table.sheetName,
      columns: table.columns.map((column) => ({ width: column.width })),
      stickyRowsCount: preamble.length + 1,
      data: [...preamble, headerRow, ...bodyRows],
    };
  });
}

/**
 * `write-excel-file` is imported here rather than at module scope so it is
 * only fetched when someone actually asks for a file. Note the `/browser`
 * subpath - the package has no root export.
 */
export async function buildReportXlsxBlob(data: ReportExportData): Promise<Blob> {
  const { default: writeXlsxFile } = await import('write-excel-file/browser');

  // The cast is needed because the library's Cell type is a wide union; the
  // shapes built above are the typed subset of it that this report uses.
  return writeXlsxFile(buildReportSheets(data) as never, {
    fontFamily: 'Calibri',
    fontSize: 11,
  }).toBlob();
}

/**
 * jsPDF and jspdf-autotable are imported here for the same reason - they are
 * by far the heaviest thing on this page.
 */
export async function buildReportPdfBlob(data: ReportExportData): Promise<Blob> {
  const { jsPDF } = await import('jspdf');
  const { default: autoTable } = await import('jspdf-autotable');

  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const margin = 40;
  let cursor = margin + 8;

  doc.setFontSize(16);
  doc.text('Rapport - Sport For Alle', margin, cursor);
  cursor += 20;
  doc.setFontSize(10);
  doc.setTextColor(90);
  doc.text(`Periode: ${data.periodLabel}`, margin, cursor);
  cursor += 14;
  doc.text(`Generert: ${formatTimestamp(data.generatedAt)}`, margin, cursor);
  doc.setTextColor(0);

  for (const table of buildReportTables(data)) {
    cursor += 30;
    doc.setFontSize(12);
    doc.text(table.title, margin, cursor);
    cursor += 14;
    doc.setFontSize(9);
    doc.setTextColor(90);
    doc.text(table.description, margin, cursor);
    doc.setTextColor(0);

    autoTable(doc, {
      startY: cursor + 8,
      head: [table.columns.map((column) => column.header)],
      body: toDisplayRows(table, data.interval),
      margin: { left: margin, right: margin },
      theme: 'grid',
      styles: { fontSize: 9, cellPadding: 5 },
      // The accent green from globals.css (--primary: #1f6e45).
      headStyles: { fillColor: [31, 110, 69], textColor: 255 },
    });

    cursor = (doc as unknown as { lastAutoTable: { finalY: number } }).lastAutoTable.finalY;
  }

  doc.setFontSize(8);
  doc.setTextColor(120);
  doc.text(
    'Alle tall er aggregerte. Rapporten inneholder ingen personopplysninger.',
    margin,
    doc.internal.pageSize.getHeight() - 24,
  );

  return doc.output('blob');
}

/** Hands the finished file to the browser's own download mechanism. */
export function downloadBlob(blob: Blob, filename: string): void {
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

export async function downloadReportXlsx(data: ReportExportData): Promise<void> {
  downloadBlob(await buildReportXlsxBlob(data), reportFilename(data.generatedAt, 'xlsx'));
}

export async function downloadReportPdf(data: ReportExportData): Promise<void> {
  downloadBlob(await buildReportPdfBlob(data), reportFilename(data.generatedAt, 'pdf'));
}
