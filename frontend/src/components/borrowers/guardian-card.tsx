'use client';

import { useState } from 'react';
import { Archive, ArchiveRestore, Pencil, Trash2, UserX } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardAction, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { ConfirmDialog } from '@/components/confirm-dialog';
import { EditGuardianDialog } from '@/components/borrowers/edit-guardian-dialog';
import { RowActionsMenu, type RowAction } from '@/components/row-actions-menu';
import { StatusBadge } from '@/components/status-badge';
import type { Guardian } from '@/types/guardian';

type LifecycleKind = 'archive' | 'restore' | 'anonymise' | 'delete';

const COPY: Record<
  LifecycleKind,
  { title: string; confirmLabel: string; destructive: boolean; description: (name: string) => string }
> = {
  archive: {
    title: 'Arkiver foresatt',
    confirmLabel: 'Arkiver',
    destructive: false,
    description: (name) => `${name} skjules fra listene, men ingenting slettes. Kan gjenopprettes når som helst.`,
  },
  restore: {
    title: 'Gjenopprett foresatt',
    confirmLabel: 'Gjenopprett',
    destructive: false,
    description: (name) => `${name} blir synlig i listene igjen.`,
  },
  anonymise: {
    title: 'Anonymiser foresatt',
    confirmLabel: 'Anonymiser',
    destructive: true,
    description: (name) =>
      `Navn, telefon og e-post til ${name} fjernes permanent, og loggede kontaktforsøk slettes. ` +
      'Barna beholdes, men uten kontaktinformasjon til foresatt. Dette kan ikke angres.',
  },
  delete: {
    title: 'Slett foresatt',
    confirmLabel: 'Slett',
    destructive: true,
    description: (name) =>
      `${name} slettes permanent. Dette er bare mulig når ingen barn er knyttet til foresatte - ` +
      'er de det, bruk Anonymiser i stedet.',
  },
};

function run(guardianId: string, kind: LifecycleKind): Promise<Response> {
  switch (kind) {
    case 'archive':
      return fetch(`/api/guardians/${guardianId}/archive`, { method: 'POST' });
    case 'restore':
      return fetch(`/api/guardians/${guardianId}/archive`, { method: 'DELETE' });
    case 'anonymise':
      return fetch(`/api/guardians/${guardianId}/anonymise`, { method: 'POST' });
    case 'delete':
      return fetch(`/api/guardians/${guardianId}`, { method: 'DELETE' });
  }
}

/**
 * The "Foresatt" card on the borrower detail page. A client component so the
 * edit and lifecycle actions can open dialogs - the guardian itself is still
 * fetched on the server.
 *
 * Deleting is offered even though it will usually be refused: a guardian
 * shown on a borrower page by definition still has a child, so the backend
 * answers 409 GuardianHasBorrowers. The refusal text explains what to do
 * instead, which is more useful than hiding the option and leaving staff
 * wondering why they cannot remove someone. See ADR-0026.
 */
export function GuardianCard({ guardian }: { guardian: Guardian }) {
  const [editing, setEditing] = useState(false);
  const [target, setTarget] = useState<LifecycleKind | null>(null);

  const actions: RowAction[] = guardian.anonymisedAt
    ? []
    : [
        guardian.archivedAt
          ? { label: 'Gjenopprett', icon: ArchiveRestore, onClick: () => setTarget('restore') }
          : { label: 'Arkiver', icon: Archive, onClick: () => setTarget('archive') },
        { label: 'Anonymiser', icon: UserX, onClick: () => setTarget('anonymise'), destructive: true },
        { label: 'Slett', icon: Trash2, onClick: () => setTarget('delete'), destructive: true },
      ];

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          Foresatt
          {guardian.anonymisedAt && <StatusBadge tone="neutral">Anonymisert</StatusBadge>}
          {guardian.archivedAt && !guardian.anonymisedAt && (
            <StatusBadge tone="warning">Arkivert</StatusBadge>
          )}
        </CardTitle>
        <CardAction>
          <div className="flex items-center gap-1">
            {!guardian.anonymisedAt && (
              <Button
                variant="ghost"
                size="icon-sm"
                aria-label="Rediger foresatt"
                onClick={() => setEditing(true)}
              >
                <Pencil />
              </Button>
            )}
            <RowActionsMenu actions={actions} label={`Handlinger for ${guardian.name}`} />
          </div>
        </CardAction>
      </CardHeader>
      <CardContent className="grid grid-cols-2 gap-x-5 gap-y-4">
        <Field label="Navn" value={guardian.name} />
        <Field label="Telefon" value={guardian.phone} />
        <Field label="E-post" value={guardian.email} />
      </CardContent>

      {editing && <EditGuardianDialog guardian={guardian} open onOpenChange={setEditing} />}

      {target && (
        <ConfirmDialog
          open
          onOpenChange={(open) => !open && setTarget(null)}
          title={COPY[target].title}
          description={COPY[target].description(guardian.name)}
          confirmLabel={COPY[target].confirmLabel}
          destructive={COPY[target].destructive}
          action={() => run(guardian.id, target)}
        />
      )}
    </Card>
  );
}

function Field({ label, value }: { label: string; value: string }) {
  return (
    <div>
      <div className="text-[11.5px] font-medium text-muted-foreground">{label}</div>
      <div className="mt-1 text-sm font-medium">{value}</div>
    </div>
  );
}
