'use client';

import Link from 'next/link';
import type { ComponentType } from 'react';
import { MoreHorizontalIcon, PencilIcon, SquareArrowOutUpRightIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuLinkItem,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';

/** An extra entry below "Åpne" and "Rediger" - archive, restore, anonymise, delete. */
export type RowAction = {
  label: string;
  onClick: () => void;
  icon?: ComponentType<{ className?: string }>;
  /** Red styling, for anything irreversible. */
  destructive?: boolean;
};

/**
 * The "..." action menu for a table row: open the full view, edit, plus any
 * lifecycle actions the entity supports.
 *
 * "Åpne" is either a link to a detail page (`openHref`) or a callback that
 * opens a dialog (`onOpen`). Equipment uses the dialog form: its four fields
 * are all already columns in the table, so a whole route for them earned
 * nothing.
 *
 * Destructive entries go through ConfirmDialog at the call site, never
 * straight from this menu - see ADR-0026.
 */
export function RowActionsMenu({
  openHref,
  onOpen,
  onEdit,
  actions = [],
  label = 'Handlinger',
}: {
  /** Where "Åpne" navigates to - the entity's full detail page. */
  openHref?: string;
  /** Called when "Åpne" is chosen, for entities shown in a dialog instead of a page. */
  onOpen?: () => void;
  /** Called when "Rediger" is chosen. Omit to hide the action (not every entity has an edit dialog yet). */
  onEdit?: () => void;
  /** Lifecycle actions appended below the standard ones. */
  actions?: RowAction[];
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
        {openHref && (
          <DropdownMenuLinkItem render={<Link href={openHref} />}>
            <SquareArrowOutUpRightIcon />
            Åpne
          </DropdownMenuLinkItem>
        )}
        {!openHref && onOpen && (
          <DropdownMenuItem onClick={onOpen}>
            <SquareArrowOutUpRightIcon />
            Åpne
          </DropdownMenuItem>
        )}
        {onEdit && (
          <DropdownMenuItem onClick={onEdit}>
            <PencilIcon />
            Rediger
          </DropdownMenuItem>
        )}
        {actions.map((action) => (
          <DropdownMenuItem
            key={action.label}
            variant={action.destructive ? 'destructive' : 'default'}
            onClick={action.onClick}
          >
            {action.icon ? <action.icon /> : null}
            {action.label}
          </DropdownMenuItem>
        ))}
      </DropdownMenuContent>
    </DropdownMenu>
  );
}

