'use client';

import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { formatBucketLabel } from '@/lib/report-period';
import type { TimelineBucket, TimelineInterval } from '@/types/report';

/**
 * The trend chart - one bar per day, week or month. Recharts needs the DOM,
 * so this is the one genuinely client-side piece of the reports page; the
 * numbers it draws are fetched on the server and passed in as props.
 *
 * Buckets with no loans arrive from the API with a count of 0 rather than
 * being left out, so a quiet month shows as a gap instead of the chart
 * closing up and hiding it.
 */
export function LoanTrendChart({
  buckets,
  interval,
}: {
  buckets: TimelineBucket[];
  interval: TimelineInterval;
}) {
  if (buckets.length === 0) {
    return (
      <p className="py-12 text-center text-sm text-muted-foreground">
        Ingen utlån er registrert i denne perioden.
      </p>
    );
  }

  const data = buckets.map((bucket) => ({
    label: formatBucketLabel(bucket.bucketStart, interval),
    count: bucket.count,
  }));

  return (
    <ResponsiveContainer width="100%" height={280}>
      <BarChart data={data} margin={{ top: 8, right: 8, bottom: 0, left: -16 }}>
        <CartesianGrid vertical={false} stroke="var(--border)" />
        <XAxis
          dataKey="label"
          tickLine={false}
          axisLine={false}
          interval="preserveStartEnd"
          minTickGap={24}
          tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }}
        />
        <YAxis
          allowDecimals={false}
          tickLine={false}
          axisLine={false}
          tick={{ fontSize: 12, fill: 'var(--muted-foreground)' }}
        />
        <Tooltip
          cursor={{ fill: 'var(--accent)' }}
          contentStyle={{
            borderRadius: 'var(--radius)',
            border: '1px solid var(--border)',
            background: 'var(--popover)',
            color: 'var(--popover-foreground)',
            fontSize: 12,
          }}
        />
        <Bar dataKey="count" name="Utlån" fill="var(--primary)" radius={[4, 4, 0, 0]} maxBarSize={56} />
      </BarChart>
    </ResponsiveContainer>
  );
}
