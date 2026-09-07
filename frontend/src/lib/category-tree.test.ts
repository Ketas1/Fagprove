import { buildCategoryTree, flattenVisible } from './category-tree';
import type { EquipmentCategory } from '@/types/equipment';

function category(id: string, name: string, parentCategoryId: string | null = null): EquipmentCategory {
  return { id, name, parentCategoryId, createdByStaffId: null, updatedByStaffId: null };
}

describe('buildCategoryTree', () => {
  it('puts categories with no parent at the root', () => {
    const tree = buildCategoryTree([category('1', 'Vintersport'), category('2', 'Sykler')]);

    expect(tree.map((n) => n.name)).toEqual(['Sykler', 'Vintersport']);
  });

  it('nests a category under its parent', () => {
    const tree = buildCategoryTree([
      category('1', 'Vintersport'),
      category('2', 'Ski', '1'),
      category('3', 'Snowboard', '1'),
    ]);

    expect(tree).toHaveLength(1);
    expect(tree[0].children.map((n) => n.name)).toEqual(['Ski', 'Snowboard']);
  });

  it('sorts every level alphabetically', () => {
    const tree = buildCategoryTree([
      category('1', 'Vintersport'),
      category('2', 'Snowboard', '1'),
      category('3', 'Ski', '1'),
    ]);

    expect(tree[0].children.map((n) => n.name)).toEqual(['Ski', 'Snowboard']);
  });

  it('falls a category with an unknown parent back to the root instead of dropping it', () => {
    const tree = buildCategoryTree([category('1', 'Foreldreløs', 'does-not-exist')]);

    expect(tree.map((n) => n.name)).toEqual(['Foreldreløs']);
  });

  it('supports more than one level of nesting', () => {
    const tree = buildCategoryTree([
      category('1', 'Vintersport'),
      category('2', 'Ski', '1'),
      category('3', 'Slalåmski', '2'),
    ]);

    expect(tree[0].children[0].children.map((n) => n.name)).toEqual(['Slalåmski']);
  });
});

describe('flattenVisible', () => {
  const tree = buildCategoryTree([
    category('1', 'Vintersport'),
    category('2', 'Ski', '1'),
    category('3', 'Sykler'),
  ]);

  it('shows only root nodes when nothing is expanded', () => {
    const visible = flattenVisible(tree, new Set());

    expect(visible.map((v) => v.node.name)).toEqual(['Sykler', 'Vintersport']);
  });

  it('shows children of an expanded node, in order, with the right depth', () => {
    const visible = flattenVisible(tree, new Set(['1']));

    expect(visible.map((v) => [v.node.name, v.depth])).toEqual([
      ['Sykler', 0],
      ['Vintersport', 0],
      ['Ski', 1],
    ]);
  });
});
