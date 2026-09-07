'use client';

import { usePathname, useRouter } from 'next/navigation';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { DatePicker } from '@/components/ui/date-picker';
import {
  defaultIntervalFor,
  intervalOptions,
  periodOptions,
  type PeriodPreset,
} from '@/lib/report-period';
import type { TimelineInterval } from '@/types/report';

/**
 * The period and bucket-size controls. Every choice is written to the URL and
 * the server component re-reads it from there, so a report is a link someone
 * can bookmark or paste to a colleague, and the reading itself stays on the
 * server - see docs/adr/0020-server-lesing-klient-skriving.md.
 */
export function ReportFilters({
  preset,
  interval,
  from,
  to,
}: {
  preset: PeriodPreset;
  interval: TimelineInterval;
  from: string;
  to: string;
}) {
  const router = useRouter();
  const pathname = usePathname();

  function apply(next: Partial<{ preset: PeriodPreset; interval: TimelineInterval; from: string; to: string }>) {
    const resolvedPreset = next.preset ?? preset;
    const params = new URLSearchParams({ period: resolvedPreset });

    // Changing the period resets the bucket size to what suits it, so picking
    // "Hele historikken" while "Dag" is selected cannot ask for a chart with
    // thousands of bars (which the API refuses outright).
    params.set('interval', next.interval ?? (next.preset ? defaultIntervalFor(next.preset) : interval));

    if (resolvedPreset === 'custom') {
      const resolvedFrom = next.from ?? from;
      const resolvedTo = next.to ?? to;
      if (resolvedFrom) params.set('from', resolvedFrom);
      if (resolvedTo) params.set('to', resolvedTo);
    }

    router.push(`${pathname}?${params.toString()}`);
  }

  return (
    <div className="flex flex-wrap items-end gap-4">
      <div className="flex flex-col gap-1.5">
        <label htmlFor="report-period" className="text-[12.5px] font-medium text-muted-foreground">
          Periode
        </label>
        <Select value={preset} onValueChange={(value) => apply({ preset: value as PeriodPreset })}>
          <SelectTrigger id="report-period" className="w-[190px]">
            <SelectValue />
          </SelectTrigger>
          <SelectContent>
            {periodOptions.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {preset === 'custom' && (
        <>
          <div className="flex flex-col gap-1.5">
            <label htmlFor="report-from" className="text-[12.5px] font-medium text-muted-foreground">
              Fra
            </label>
            <DatePicker
              id="report-from"
              value={from}
              onChange={(value) => apply({ from: value })}
              className="w-[170px]"
            />
          </div>
          <div className="flex flex-col gap-1.5">
            <label htmlFor="report-to" className="text-[12.5px] font-medium text-muted-foreground">
              Til
            </label>
            <DatePicker
              id="report-to"
              value={to}
              onChange={(value) => apply({ to: value })}
              className="w-[170px]"
            />
          </div>
        </>
      )}

      <div className="flex flex-col gap-1.5">
        <span className="text-[12.5px] font-medium text-muted-foreground">Vis per</span>
        <Tabs value={interval} onValueChange={(value) => apply({ interval: value as TimelineInterval })}>
          <TabsList>
            {intervalOptions.map((option) => (
              <TabsTrigger key={option.value} value={option.value}>
                {option.label}
              </TabsTrigger>
            ))}
          </TabsList>
        </Tabs>
      </div>
    </div>
  );
}
