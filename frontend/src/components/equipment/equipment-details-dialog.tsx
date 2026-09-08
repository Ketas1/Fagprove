'use client';

import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { StatusBadge } from '@/components/status-badge';
import { equipmentConditionLabel, equipmentStatusInfo } from '@/lib/status-labels';
import type { Equipment } from '@/types/equipment';

/**
 * Replaces the former /dashboard/equipment/[id] page. Every field it shows is
 * already a column in the equipment table, so a separate route added a
 * navigation step without adding information. "Rediger" opens
 * EditEquipmentDialog instead.
 */
export function EquipmentDetailsDialog({
  equipment,
  open,
  onOpenChange,
  onEdit,
}: {
  equipment: Equipment;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onEdit: () => void;
}) {
  const { label: statusLabel, tone: statusTone } = equipmentStatusInfo(equipment.status);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{equipment.name}</DialogTitle>
          <DialogDescription>
            {equipment.categoryName} · #{equipment.serialNumber}
          </DialogDescription>
        </DialogHeader>

        <div className="grid grid-cols-2 gap-x-5 gap-y-4">
          <Field label="Kategori" value={equipment.categoryName} />
          <Field label="Serienummer" value={equipment.serialNumber} />
          <Field label="Tilstand" value={equipmentConditionLabel(equipment.condition)} />
          <div>
            <div className="text-[11.5px] font-medium text-muted-foreground">Status</div>
            <div className="mt-1">
              <StatusBadge tone={statusTone}>{statusLabel}</StatusBadge>
            </div>
          </div>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Lukk
          </Button>
          <Button onClick={onEdit}>Rediger</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
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
