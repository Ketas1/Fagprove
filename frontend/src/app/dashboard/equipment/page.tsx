import { EquipmentExplorer } from '@/components/equipment/equipment-explorer';
import { fetchBackend } from '@/lib/backend';
import type { Equipment, EquipmentCategory } from '@/types/equipment';

export const dynamic = 'force-dynamic';

export default async function EquipmentPage() {
  const [equipment, categories] = await Promise.all([
    fetchBackend<Equipment[]>(['equipment']),
    fetchBackend<EquipmentCategory[]>(['equipment-categories']),
  ]);

  return <EquipmentExplorer equipment={equipment} categories={categories} />;
}
