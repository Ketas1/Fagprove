import type { ReactNode } from 'react';
import { Info } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Tooltip, TooltipContent, TooltipTrigger } from '@/components/ui/tooltip';

/**
 * Marks a piece of the approved design that has no backend support yet (see
 * the gap list in docs/13-frontend-designsystem.md) - contact attempts,
 * photo storage, the audit log, ban/unban, staff invites. Keeps the full
 * design navigable and visible rather than quietly deleting the parts that
 * can't be wired up today.
 */
export function NotBuiltYetBadge({ reason }: { reason: string }) {
  return (
    <Tooltip>
      <TooltipTrigger>
        <Badge variant="outline" className="gap-1 text-muted-foreground">
          <Info className="size-3" />
          Ikke bygget ennå
        </Badge>
      </TooltipTrigger>
      <TooltipContent>{reason}</TooltipContent>
    </Tooltip>
  );
}

/** Same idea as {@link NotBuiltYetBadge}, but for an action button the design shows with nothing behind it yet. */
export function NotBuiltYetButton({ reason, children }: { reason: string; children: ReactNode }) {
  return (
    <Tooltip>
      <TooltipTrigger render={<Button variant="outline" disabled />}>{children}</TooltipTrigger>
      <TooltipContent>{reason}</TooltipContent>
    </Tooltip>
  );
}
