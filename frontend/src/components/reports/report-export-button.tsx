'use client';

import { useState } from 'react';
import { DownloadIcon, FileSpreadsheetIcon, FileTextIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { downloadReportPdf, downloadReportXlsx, type ReportExportData } from '@/lib/report-export';
import type { AgeGroupFigures, LoanFigures, TimelineBucket, TimelineInterval } from '@/types/report';

/**
 * Both files are built in the browser from the aggregated numbers already on
 * the page - there is no export endpoint, and nothing per-loan is ever
 * fetched to produce them. See docs/adr/0023-rapporteksport.md.
 */
export function ReportExportButton({
  periodLabel,
  interval,
  summary,
  ageGroups,
  timeline,
}: {
  periodLabel: string;
  interval: TimelineInterval;
  summary: LoanFigures;
  ageGroups: AgeGroupFigures[];
  timeline: TimelineBucket[];
}) {
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  function exportData(): ReportExportData {
    return { periodLabel, generatedAt: new Date(), interval, summary, ageGroups, timeline };
  }

  async function exportFile(format: 'Excel' | 'PDF') {
    setError(null);
    setBusy(true);
    try {
      await (format === 'Excel' ? downloadReportXlsx(exportData()) : downloadReportPdf(exportData()));
    } catch (cause) {
      console.error(`${format}-eksport feilet`, cause);
      setError(`Kunne ikke lage ${format}-filen.`);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="flex flex-col items-end gap-1.5">
      <DropdownMenu>
        <DropdownMenuTrigger
          render={
            <Button variant="outline" disabled={busy}>
              <DownloadIcon />
              {busy ? 'Lager fil …' : 'Eksporter'}
            </Button>
          }
        />
        <DropdownMenuContent>
          <DropdownMenuItem onClick={() => exportFile('Excel')}>
            <FileSpreadsheetIcon />
            Last ned Excel
          </DropdownMenuItem>
          <DropdownMenuItem onClick={() => exportFile('PDF')}>
            <FileTextIcon />
            Last ned PDF
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      {error && <p className="text-xs text-status-danger-fg">{error}</p>}
    </div>
  );
}
