import Link from 'next/link';
import { notFound } from 'next/navigation';
import { ArrowLeft } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { StatusBadge } from '@/components/status-badge';
import { BackendError, fetchBackend } from '@/lib/backend';
import { equipmentConditionLabel, equipmentStatusInfo } from '@/lib/status-labels';
import type { Equipment } from '@/types/equipment';

export const dynamic = 'force-dynamic';

export default async function EquipmentDetailPage({ params }: { params: Promise<{ id: string }> }) {
  const { id } = await params;

  let equipment: Equipment;

  try {
    equipment = await fetchBackend<Equipment>(['equipment', id]);
  } catch (error) {
    if (error instanceof BackendError && error.status === 404) {
      notFound();
    }
    throw error;
  }

  const { label: statusLabel, tone: statusTone } = equipmentStatusInfo(equipment.status);

  return (
    <div className="flex flex-col gap-4">
      <Link
        href="/dashboard/equipment"
        className="flex w-fit items-center gap-1.5 text-[12.5px] font-medium text-muted-foreground"
      >
        <ArrowLeft className="size-3.5" /> Tilbake til utstyr
      </Link>

      <div className="flex items-center gap-2.5">
        <h1 className="text-xl font-semibold tracking-tight">{equipment.name}</h1>
        <StatusBadge tone={statusTone} className="h-[26px] px-3 text-[12.5px]">
          {statusLabel}
        </StatusBadge>
      </div>
      <p className="-mt-2 text-[12.5px] text-muted-foreground">
        {equipment.categoryName} · #{equipment.serialNumber}
      </p>

      <Card className="max-w-md">
        <CardHeader>
          <CardTitle>Detaljer</CardTitle>
        </CardHeader>
        <CardContent className="grid grid-cols-2 gap-x-5 gap-y-4">
          <Field label="Kategori" value={equipment.categoryName} />
          <Field label="Serienummer" value={equipment.serialNumber} />
          <Field label="Tilstand" value={equipmentConditionLabel(equipment.condition)} />
          <Field label="Status" value={statusLabel} />
        </CardContent>
      </Card>
    </div>
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
