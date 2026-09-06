'use client';

import { useMemo, useState } from 'react';
import { CategoryTree } from '@/components/equipment/category-tree';
import { EquipmentExplorer } from '@/components/equipment/equipment-explorer';
import type { Equipment, EquipmentCategory } from '@/types/equipment';

export function EquipmentWorkspace({
  equipment,
  categories,
}: {
  equipment: Equipment[];
  categories: EquipmentCategory[];
}) {
  const [selectedCategoryId, setSelectedCategoryId] = useState<string | null>(null);

  const equipmentCountByCategory = useMemo(() => {
    const counts = new Map<string, number>();
    for (const item of equipment) {
      counts.set(item.categoryId, (counts.get(item.categoryId) ?? 0) + 1);
    }
    return counts;
  }, [equipment]);

  const selectedCategoryName =
    categories.find((category) => category.id === selectedCategoryId)?.name ?? null;

  return (
    <div className="flex flex-1 gap-6">
      <CategoryTree
        categories={categories}
        equipmentCountByCategory={equipmentCountByCategory}
        selectedCategoryId={selectedCategoryId}
        onSelect={setSelectedCategoryId}
      />
      <EquipmentExplorer
        equipment={equipment}
        categories={categories}
        selectedCategoryId={selectedCategoryId}
        selectedCategoryName={selectedCategoryName}
      />
    </div>
  );
}
