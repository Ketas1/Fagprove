'use client';

import Link from 'next/link';
import { MoreHorizontalIcon, PencilIcon, SquareArrowOutUpRightIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLinkItem,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

/**
 * The "..." action menu for a table row: open the full detail view, or edit.
 * Archiving is deliberately not offered here - no entity supports it yet,
 * see docs/13-frontend-designsystem.md.
 */
export function RowActionsMenu({
  openHref,
  onEdit,
  label = 'Handlinger',
}: {
  /** Where "Åpne" navigates to - the entity's full detail page. */
  openHref: string;
  /** Called when "Rediger" is chosen. Omit to hide the action (not every entity has an edit dialog yet). */
  onEdit?: () => void;
  /** Accessible label for the trigger button, e.g. "Handlinger for Ola Nordmann". */
  label?: string;
}) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        render={
          <Button variant="ghost" size="icon" aria-label={label} onClick={(event) => event.stopPropagation()}>
            <MoreHorizontalIcon />
          </Button>
        }
      />
      <DropdownMenuContent onClick={(event) => event.stopPropagation()}>
        <DropdownMenuLinkItem render={<Link href={openHref} />}>
          <SquareArrowOutUpRightIcon />
          Åpne
        </DropdownMenuLinkItem>
        {onEdit && (
          <DropdownMenuItem onClick={onEdit}>
            <PencilIcon />
            Rediger
          </DropdownMenuItem>
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}
