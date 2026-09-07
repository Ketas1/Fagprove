import type { EquipmentCategory } from '@/types/equipment';

export type CategoryNode = EquipmentCategory & { children: CategoryNode[] };

/**
 * Builds a nested tree from the flat list the backend returns. A category
 * whose `parentCategoryId` doesn't match any known category (shouldn't
 * happen given the backend's own foreign key, but the frontend doesn't need
 * to trust that) falls back to the root rather than being dropped silently.
 */
export function buildCategoryTree(categories: EquipmentCategory[]): CategoryNode[] {
  const byId = new Map<string, CategoryNode>();

  for (const category of categories) {
    byId.set(category.id, { ...category, children: [] });
  }

  const roots: CategoryNode[] = [];

  for (const category of categories) {
    const node = byId.get(category.id)!;
    const parent = category.parentCategoryId ? byId.get(category.parentCategoryId) : undefined;

    if (parent) {
      parent.children.push(node);
    } else {
      roots.push(node);
    }
  }

  sortByName(roots);

  return roots;
}

function sortByName(nodes: CategoryNode[]): void {
  nodes.sort((a, b) => a.name.localeCompare(b.name, 'nb'));

  for (const node of nodes) {
    sortByName(node.children);
  }
}

/** Flattens a tree back into `{ node, depth }` pairs in display order, skipping collapsed subtrees. */
export function flattenVisible(
  nodes: CategoryNode[],
  expandedIds: ReadonlySet<string>,
  depth = 0,
): { node: CategoryNode; depth: number }[] {
  const result: { node: CategoryNode; depth: number }[] = [];

  for (const node of nodes) {
    result.push({ node, depth });

    if (node.children.length > 0 && expandedIds.has(node.id)) {
      result.push(...flattenVisible(node.children, expandedIds, depth + 1));
    }
  }

  return result;
}
