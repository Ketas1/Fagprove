'use client';

import { useMemo, useState } from 'react';
import { Search } from 'lucide-react';
import { StatusBadge } from '@/components/status-badge';
import { Input } from '@/components/ui/input';
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { NewEquipmentDialog } from '@/components/equipment/new-equipment-dialog';
import { equipmentConditionLabel, equipmentStatusInfo } from '@/lib/status-labels';
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
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<StatusFilter>('all');

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
    const query = search.trim().toLowerCase();
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
          <div className="relative">
            <Search className="pointer-events-none absolute top-1/2 left-2.5 size-3.5 -translate-y-1/2 text-muted-foreground" />
            <Input
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Søk utstyr eller serienummer…"
              className="w-64 pl-8"
            />
          </div>
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

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Navn</TableHead>
            <TableHead>Kategori</TableHead>
            <TableHead>Serienummer</TableHead>
            <TableHead>Tilstand</TableHead>
            <TableHead>Status</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {filtered.length === 0 && (
            <TableRow>
              <TableCell colSpan={5} className="py-8 text-center text-sm text-muted-foreground">
                Ingen utstyr matcher søket.
              </TableCell>
            </TableRow>
          )}
          {filtered.map((item) => {
            const { label, tone } = equipmentStatusInfo(item.status);

            return (
              <TableRow key={item.id}>
                <TableCell className="font-medium">{item.name}</TableCell>
                <TableCell>{item.categoryName}</TableCell>
                <TableCell className="text-muted-foreground">#{item.serialNumber}</TableCell>
                <TableCell>{equipmentConditionLabel(item.condition)}</TableCell>
                <TableCell>
                  <StatusBadge tone={tone}>{label}</StatusBadge>
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </div>
  );
}
