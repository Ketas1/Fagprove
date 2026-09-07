'use client';

import { useRouter } from 'next/navigation';
import { useMemo, useState } from 'react';
import { RowActionsMenu } from '@/components/row-actions-menu';
import { StatusBadge } from '@/components/status-badge';
import { SearchInput } from '@/components/ui/search-input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { EditEquipmentDialog } from '@/components/equipment/edit-equipment-dialog';
import { NewEquipmentDialog } from '@/components/equipment/new-equipment-dialog';
import { equipmentConditionLabel, equipmentStatusInfo } from '@/lib/status-labels';
import { normalizeSearchQuery } from '@/lib/search';
import type { Equipment, EquipmentCategory, EquipmentStatus } from '@/types/equipment';

type StatusFilter = 'all' | EquipmentStatus;

export function EquipmentExplorer({
  equipment,
  categories,
  selectedCategoryId,
  selectedCategoryName,
}: {
  equipment: Equipment[];
  categories: EquipmentCategory[];
  selectedCategoryId: string | null;
  selectedCategoryName: string | null;
}) {
  const router = useRouter();
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');
  const [editingEquipment, setEditingEquipment] = useState<Equipment | null>(null);

  const inCategory = useMemo(
    () => (selectedCategoryId ? equipment.filter((item) => item.categoryId === selectedCategoryId) : equipment),
    [equipment, selectedCategoryId],
  );

  const counts = useMemo(() => {
    const result: Record<StatusFilter, number> = {
      all: inCategory.length,
      Available: 0,
      OnLoan: 0,
      OutOfService: 0,
      WrittenOff: 0,
    };
    for (const item of inCategory) {
      result[item.status] += 1;
    }
    return result;
  }, [inCategory]);

  const filtered = useMemo(() => {
    const query = normalizeSearchQuery(search);
    return inCategory.filter((item) => {
      const matchesStatus = statusFilter === 'all' || item.status === statusFilter;
      const matchesSearch =
        query.length === 0 ||
        item.name.toLowerCase().includes(query) ||
        item.serialNumber.toLowerCase().includes(query);
      return matchesStatus && matchesSearch;
    });
  }, [inCategory, statusFilter, search]);

  return (
    <div className="flex flex-1 flex-col gap-4">
      <div className="text-[12.5px] font-medium text-muted-foreground">
        {selectedCategoryName ? `Utstyr i «${selectedCategoryName}»` : 'Alt utstyr'}
      </div>
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex flex-wrap items-center gap-2.5">
          <SearchInput value={search} onChange={setSearch} placeholder="Søk utstyr eller serienummer…" />
          <Tabs value={statusFilter} onValueChange={(value) => setStatusFilter(value as StatusFilter)}>
            <TabsList>
              <TabsTrigger value="all">Alle ({counts.all})</TabsTrigger>
              <TabsTrigger value="Available">Ledig ({counts.Available})</TabsTrigger>
              <TabsTrigger value="OnLoan">Utlånt ({counts.OnLoan})</TabsTrigger>
              <TabsTrigger value="OutOfService">Ute av drift ({counts.OutOfService})</TabsTrigger>
              <TabsTrigger value="WrittenOff">Avskrevet ({counts.WrittenOff})</TabsTrigger>
            </TabsList>
          </Tabs>
        </div>
        <NewEquipmentDialog categories={categories} initialCategoryId={selectedCategoryId} />
      </div>

      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[24%]">Navn</TableHead>
            <TableHead className="w-[18%]">Kategori</TableHead>
            <TableHead className="w-[18%]">Serienummer</TableHead>
            <TableHead className="w-[15%]">Tilstand</TableHead>
            <TableHead className="w-[15%]">Status</TableHead>
            <TableHead className="w-[10%]">
              <span className="sr-only">Handlinger</span>
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filtered.length === 0 && (
            <TableRow>
              <TableCell colSpan={6} className="py-8 text-center text-sm text-muted-foreground">
                Ingen utstyr matcher søket.
              </TableCell>
            </TableRow>
          )}
          {filtered.map((item) => {
            const { label, tone } = equipmentStatusInfo(item.status);
            const detailHref = `/dashboard/equipment/${item.id}`;

            return (
              <TableRow
                key={item.id}
                onClick={() => router.push(detailHref)}
                className="cursor-pointer hover:bg-muted/40"
              >
                <TableCell className="max-w-0 truncate font-medium">{item.name}</TableCell>
                <TableCell className="max-w-0 truncate">{item.categoryName}</TableCell>
                <TableCell className="max-w-0 truncate text-muted-foreground">#{item.serialNumber}</TableCell>
                <TableCell>{equipmentConditionLabel(item.condition)}</TableCell>
                <TableCell>
                  <StatusBadge tone={tone}>{label}</StatusBadge>
                </TableCell>
                <TableCell onClick={(event) => event.stopPropagation()}>
                  <RowActionsMenu
                    openHref={detailHref}
                    onEdit={() => setEditingEquipment(item)}
                    label={`Handlinger for ${item.name}`}
                  />
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>

      {editingEquipment && (
        <EditEquipmentDialog
          equipment={editingEquipment}
          categories={categories}
          open
          onOpenChange={(open) => !open && setEditingEquipment(null)}
        />
      )}
    </div>
  );
}
