'use client';

import { useMemo, useState } from 'react';
import { ChevronRight, FolderTree, Pencil, Plus, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';
import { buildCategoryTree, flattenVisible, type CategoryNode } from '@/lib/category-tree';
import { CreateCategoryDialog, DeleteCategoryDialog, RenameCategoryDialog } from '@/components/equipment/category-dialogs';
import type { EquipmentCategory } from '@/types/equipment';

type DialogState =
  | { type: 'create'; parentCategoryId: string | null; parentName: string | null }
  | { type: 'rename'; category: EquipmentCategory }
  | { type: 'delete'; category: EquipmentCategory }
  | null;

export function CategoryTree({
  categories,
  equipmentCountByCategory,
  selectedCategoryId,
  onSelect,
}: {
  categories: EquipmentCategory[];
  equipmentCountByCategory: Map<string, number>;
  selectedCategoryId: string | null;
  onSelect: (id: string | null) => void;
}) {
  const tree = useMemo(() => buildCategoryTree(categories), [categories]);
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [dialog, setDialog] = useState<DialogState>(null);

  const visible = useMemo(() => flattenVisible(tree, expandedIds), [tree, expandedIds]);

  function toggleExpand(id: string) {
    setExpandedIds((current) => {
      const next = new Set(current);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  return (
    <div className="flex w-64 shrink-0 flex-col gap-2">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-1.5 text-[12.5px] font-medium text-muted-foreground">
          <FolderTree className="size-3.5" /> Kategorier
        </div>
        <Button
          variant="ghost"
          size="icon-sm"
          onClick={() => setDialog({ type: 'create', parentCategoryId: null, parentName: null })}
        >
          <Plus />
        </Button>
      </div>

      <button
        onClick={() => onSelect(null)}
        className={cn(
          'rounded-md px-2 py-1.5 text-left text-sm text-muted-foreground hover:bg-muted',
          selectedCategoryId === null && 'bg-muted font-medium text-foreground',
        )}
      >
        Alt utstyr
      </button>

      <div className="flex flex-col gap-0.5">
        {visible.map(({ node, depth }) => (
          <CategoryRow
            key={node.id}
            node={node}
            depth={depth}
            isExpanded={expandedIds.has(node.id)}
            isSelected={selectedCategoryId === node.id}
            equipmentCount={equipmentCountByCategory.get(node.id) ?? 0}
            onToggleExpand={() => toggleExpand(node.id)}
            onSelect={() => onSelect(node.id)}
            onAddChild={() => setDialog({ type: 'create', parentCategoryId: node.id, parentName: node.name })}
            onRename={() => setDialog({ type: 'rename', category: node })}
            onDelete={() => setDialog({ type: 'delete', category: node })}
          />
        ))}
      </div>

      {dialog?.type === 'create' && (
        <CreateCategoryDialog
          open
          onOpenChange={(open) => !open && setDialog(null)}
          parentCategoryId={dialog.parentCategoryId}
          parentName={dialog.parentName}
        />
      )}
      {dialog?.type === 'rename' && (
        <RenameCategoryDialog open onOpenChange={(open) => !open && setDialog(null)} category={dialog.category} />
      )}
      {dialog?.type === 'delete' && (
        <DeleteCategoryDialog
          open
          onOpenChange={(open) => !open && setDialog(null)}
          category={dialog.category}
          onDeleted={() => {
            if (selectedCategoryId === dialog.category.id) {
              onSelect(null);
            }
          }}
        />
      )}
    </div>
  );
}

function CategoryRow({
  node,
  depth,
  isExpanded,
  isSelected,
  equipmentCount,
  onToggleExpand,
  onSelect,
  onAddChild,
  onRename,
  onDelete,
}: {
  node: CategoryNode;
  depth: number;
  isExpanded: boolean;
  isSelected: boolean;
  equipmentCount: number;
  onToggleExpand: () => void;
  onSelect: () => void;
  onAddChild: () => void;
  onRename: () => void;
  onDelete: () => void;
}) {
  const hasChildren = node.children.length > 0;

  return (
    <div
      className={cn(
        'flex items-center gap-1 rounded-md py-1 pr-1 text-sm hover:bg-muted/60',
        isSelected && 'bg-muted font-medium',
      )}
      style={{ paddingLeft: depth * 16 + 4 }}
    >
      <button
        onClick={onToggleExpand}
        className={cn('flex size-5 shrink-0 items-center justify-center text-muted-foreground', !hasChildren && 'invisible')}
      >
        <ChevronRight className={cn('size-3.5 transition-transform', isExpanded && 'rotate-90')} />
      </button>
      <button onClick={onSelect} className="flex flex-1 items-center gap-1.5 truncate text-left">
        {node.name}
        <span className="text-xs text-muted-foreground">({equipmentCount})</span>
      </button>
      <Button variant="ghost" size="icon-xs" onClick={onAddChild} title="Ny underkategori">
        <Plus />
      </Button>
      <Button variant="ghost" size="icon-xs" onClick={onRename} title="Gi nytt navn">
        <Pencil />
      </Button>
      <Button variant="ghost" size="icon-xs" onClick={onDelete} title="Slett">
        <Trash2 />
      </Button>
    </div>
  );
}
