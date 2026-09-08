import { AlertTriangleIcon } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { LoanTrendChart } from '@/components/reports/loan-trend-chart';
import { ReportExportButton } from '@/components/reports/report-export-button';
import { ReportFilters } from '@/components/reports/report-filters';
import { BackendError, fetchBackend } from '@/lib/backend';
import {
  defaultIntervalFor,
  formatPeriodLabel,
  isPeriodPreset,
  isTimelineInterval,
  resolvePeriodRange,
  type PeriodPreset,
} from '@/lib/report-period';
import type {
  AgeGroupReport,
  LoanFigures,
  LoanSummaryReport,
  LoanTimelineReport,
} from '@/types/report';

export const dynamic = 'force-dynamic';

/// Half a year is the span the municipality asks about most often, so it is
/// what the page opens on rather than an empty state or the whole history.
const DEFAULT_PRESET: PeriodPreset = 'last6m';

/** The five figures, in the order they are read, with the tone each carries. */
const summaryCards = [
  { key: 'totalLoans', label: 'Utlån totalt', tone: '' },
  { key: 'returnedOnTime', label: 'Levert i tide', tone: 'text-status-success-fg' },
  { key: 'returnedLate', label: 'Levert for sent', tone: 'text-status-warning-fg' },
  { key: 'notReturned', label: 'Ikke levert', tone: 'text-status-danger-fg' },
  { key: 'stillActive', label: 'Fortsatt aktive', tone: 'text-status-info-fg' },
] as const satisfies readonly { key: keyof LoanFigures; label: string; tone: string }[];

function share(value: number, total: number): string {
  return total === 0 ? '0 %' : `${Math.round((value / total) * 100)} %`;
}

export default async function RapporterPage({
  searchParams,
}: {
  searchParams: Promise<{ period?: string; interval?: string; from?: string; to?: string }>;
}) {
  const params = await searchParams;
  const preset = isPeriodPreset(params.period) ? params.period : DEFAULT_PRESET;
  const custom = { from: params.from ?? null, to: params.to ?? null };
  const range = resolvePeriodRange(preset, new Date(), custom);
  const interval = isTimelineInterval(params.interval) ? params.interval : defaultIntervalFor(preset);

  const periodSearch = new URLSearchParams();
  if (range.from) periodSearch.set('from', range.from);
  if (range.to) periodSearch.set('to', range.to);

  const timelineSearch = new URLSearchParams(periodSearch);
  timelineSearch.set('interval', interval);

  const [summary, ageGroups, timeline] = await Promise.all([
    fetchBackend<LoanSummaryReport>(['reports', 'loans'], periodSearch.toString()),
    fetchBackend<AgeGroupReport>(['reports', 'age-groups'], periodSearch.toString()),
    fetchTimeline(timelineSearch.toString()),
  ]);

  const total = summary.figures.totalLoans;
  const periodLabel = formatPeriodLabel(range);
  const ageGroupSum = sumFigures(ageGroups.groups.map((group) => group.figures));

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardContent className="flex flex-wrap items-end justify-between gap-4 pt-0">
          <ReportFilters
            preset={preset}
            interval={interval}
            from={custom.from ?? ''}
            to={custom.to ?? ''}
          />
          <ReportExportButton
            periodLabel={periodLabel}
            interval={interval}
            summary={summary.figures}
            ageGroups={ageGroups.groups}
            timeline={timeline.report?.buckets ?? []}
          />
        </CardContent>
      </Card>

      <div className="grid grid-cols-5 gap-4">
        {summaryCards.map((card) => (
          <Card key={card.key}>
            <CardContent className="flex flex-col gap-1.5 pt-0">
              <span className="text-[12.5px] font-medium text-muted-foreground">{card.label}</span>
              <span className={`text-[26px] font-semibold tracking-tight ${card.tone}`}>
                {summary.figures[card.key]}
              </span>
              <span className="text-xs text-muted-foreground">
                {card.key === 'totalLoans' ? periodLabel : `${share(summary.figures[card.key], total)} av totalen`}
              </span>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Utvikling over tid</CardTitle>
        </CardHeader>
        <CardContent>
          {timeline.error ? (
            <div className="flex flex-col items-center gap-2 py-12 text-center text-sm text-muted-foreground">
              <AlertTriangleIcon className="size-5" />
              {timeline.error}
            </div>
          ) : (
            <LoanTrendChart buckets={timeline.report?.buckets ?? []} interval={interval} />
          )}
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Utlån per aldersgruppe</CardTitle>
          <p className="text-[12.5px] text-muted-foreground">
            Alder er regnet på utlånstidspunktet, ikke i dag.
          </p>
        </CardHeader>
        <CardContent>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Aldersgruppe</TableHead>
                <TableHead className="text-right">Utlån totalt</TableHead>
                <TableHead className="text-right">Levert i tide</TableHead>
                <TableHead className="text-right">Levert for sent</TableHead>
                <TableHead className="text-right">Ikke levert</TableHead>
                <TableHead className="text-right">Fortsatt aktive</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {ageGroups.groups.map((group) => (
                <TableRow key={group.ageGroup}>
                  <TableCell className="font-medium">{group.ageGroup} år</TableCell>
                  <TableCell className="text-right">{group.figures.totalLoans}</TableCell>
                  <TableCell className="text-right">{group.figures.returnedOnTime}</TableCell>
                  <TableCell className="text-right">{group.figures.returnedLate}</TableCell>
                  <TableCell className="text-right">{group.figures.notReturned}</TableCell>
                  <TableCell className="text-right">{group.figures.stillActive}</TableCell>
                </TableRow>
              ))}
              <TableRow className="font-semibold">
                <TableCell>Sum</TableCell>
                <TableCell className="text-right">{ageGroupSum.totalLoans}</TableCell>
                <TableCell className="text-right">{ageGroupSum.returnedOnTime}</TableCell>
                <TableCell className="text-right">{ageGroupSum.returnedLate}</TableCell>
                <TableCell className="text-right">{ageGroupSum.notReturned}</TableCell>
                <TableCell className="text-right">{ageGroupSum.stillActive}</TableCell>
              </TableRow>
            </TableBody>
          </Table>
          <p className="pt-4 text-xs text-muted-foreground">
            Alle tall er aggregerte og viser ingen enkeltpersoner. Summen av de fire siste kolonnene
            er alltid lik «Utlån totalt», fordi hvert utlån har nøyaktig én av disse tilstandene.
          </p>
        </CardContent>
      </Card>
    </div>
  );
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
 * The only request on this page that can legitimately fail on a valid-looking
 * choice: a long period at day granularity exceeds the API's bucket limit and
 * comes back as a 400. That is a message about the filter, not a broken page,
 * so it is caught here and the rest of the report still renders.
 */
async function fetchTimeline(
  search: string,
): Promise<{ report: LoanTimelineReport | null; error: string | null }> {
  try {
    return { report: await fetchBackend<LoanTimelineReport>(['reports', 'timeline'], search), error: null };
  } catch (cause) {
    if (cause instanceof BackendError && cause.status === 400) {
      return {
        report: null,
        error: 'Perioden gir for mange søyler med denne oppdelingen. Velg uke eller måned i stedet.',
      };
    }

    throw cause;
  }
}
