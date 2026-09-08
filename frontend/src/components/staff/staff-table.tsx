'use client';

import { useState } from 'react';
import { Trash2 } from 'lucide-react';
import { ConfirmDialog } from '@/components/confirm-dialog';
import { RowActionsMenu } from '@/components/row-actions-menu';
import { StatusBadge } from '@/components/status-badge';
import { EditStaffDialog } from '@/components/staff/edit-staff-dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import type { Staff } from '@/types/staff';

/**
 * Client component only because the row actions open an edit dialog - the
 * data itself is still fetched on the server in app/dashboard/staff/page.tsx.
 * "Konto" shows whether the profile has been claimed by an Auth0 account; the
 * Auth0 id itself is never rendered.
 */
export function StaffTable({ staff }: { staff: Staff[] }) {
  const [editing, setEditing] = useState<Staff | null>(null);
  const [deleting, setDeleting] = useState<Staff | null>(null);

  if (staff.length === 0) {
    return <p className="py-6 text-center text-sm text-muted-foreground">Ingen ansattprofiler ennå.</p>;
  }

  return (
    <>
      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[22%]">Navn</TableHead>
            <TableHead className="w-[20%]">Stilling</TableHead>
            <TableHead className="w-[16%]">Telefon</TableHead>
            <TableHead className="w-[26%]">E-post</TableHead>
            <TableHead className="w-[10%]">Konto</TableHead>
            <TableHead className="w-[6%]">
              <span className="sr-only">Handlinger</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {staff.map((member) => (
            <TableRow key={member.id}>
              <TableCell className="max-w-0 truncate font-medium">{member.name}</TableCell>
              <TableCell className="max-w-0 truncate">
                {member.jobTitle ?? <span className="text-muted-foreground">–</span>}
              </TableCell>
              <TableCell className="max-w-0 truncate">
                {member.phone ? (
                  <a href={`tel:${member.phone}`} className="hover:underline">
                    {member.phone}
                  </a>
                ) : (
                  <span className="text-muted-foreground">–</span>
                )}
              </TableCell>
              <TableCell className="max-w-0 truncate">
                {member.email ? (
                  <a href={`mailto:${member.email}`} className="hover:underline">
                    {member.email}
                  </a>
                ) : (
                  <span className="text-muted-foreground">–</span>
                )}
              </TableCell>
              <TableCell>
                {member.auth0UserId ? (
                  <StatusBadge tone="success">Koblet</StatusBadge>
                ) : (
                  <StatusBadge tone="neutral">Ikke koblet</StatusBadge>
                )}
              </TableCell>
              <TableCell>
                <RowActionsMenu
                  onEdit={() => setEditing(member)}
                  actions={[
                    {
                      label: 'Slett',
                      icon: Trash2,
                      destructive: true,
                      onClick: () => setDeleting(member),
                    },
                  ]}
                  label={`Handlinger for ${member.name}`}
                />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      {deleting && (
        <ConfirmDialog
          open
          onOpenChange={(open) => !open && setDeleting(null)}
          title="Slett ansatt"
          description={`Ansattprofilen til ${deleting.name} slettes permanent. Historikk de har registrert beholdes, men uten navn på hvem som utførte den.`}
          confirmLabel="Slett"
          destructive
          action={() => fetch(`/api/staff/${deleting.id}`, { method: 'DELETE' })}
        />
      )}

      {editing && (
        <EditStaffDialog
          staff={editing}
          open
          onOpenChange={(open) => !open && setEditing(null)}
        />
      )}
    </>
  );
}
