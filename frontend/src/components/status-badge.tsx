import type { ReactNode } from 'react';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

/// The four semantic tones from docs/13-frontend-designsystem.md, plus a
/// neutral fallback and the filled "solid" treatment reserved for a final,
/// no-longer-changing state (Lost/WrittenOff).
export type StatusTone = 'danger' | 'warning' | 'success' | 'info' | 'neutral' | 'solid';

const toneClasses: Record<StatusTone, string> = {
  danger: 'bg-status-danger-bg text-status-danger-fg',
  warning: 'bg-status-warning-bg text-status-warning-fg',
  success: 'bg-status-success-bg text-status-success-fg',
  info: 'bg-status-info-bg text-status-info-fg',
  neutral: 'bg-muted text-muted-foreground',
  solid: 'bg-foreground text-background',
};

export function StatusBadge({
  tone,
  children,
  className,
}: {
  tone: StatusTone;
  children: ReactNode;
  className?: string;
}) {
  return (
    <Badge variant="outline" className={cn('border-transparent', toneClasses[tone], className)}>
      {children}
    </Badge>
  );
}
