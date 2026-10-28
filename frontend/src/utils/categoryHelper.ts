import type { Category } from "../core/entities/Category";

export interface HierarchicalCategory extends Category {
  subcategories: HierarchicalCategory[];
}

/**
 * Organizes a flat list of categories into a hierarchical tree structure.
 * Root categories (no parentCategoryId) become top-level, with their
 * children nested under `subcategories`.
 */
export function organizeCategories(categories: Category[]): HierarchicalCategory[] {
  const map = new Map<string, HierarchicalCategory>();

  // Create a HierarchicalCategory wrapper for each category
  for (const cat of categories) {
    map.set(cat.id, { ...cat, subcategories: [] });
  }

  const roots: HierarchicalCategory[] = [];

  for (const cat of map.values()) {
    if (cat.parentCategoryId && map.has(cat.parentCategoryId)) {
      map.get(cat.parentCategoryId)!.subcategories.push(cat);
    } else {
      roots.push(cat);
    }
  }

  // Sort alphabetically
  const sortByName = (a: HierarchicalCategory, b: HierarchicalCategory) =>
    a.name.localeCompare(b.name);

  roots.sort(sortByName);
  for (const root of roots) {
    root.subcategories.sort(sortByName);
  }

  return roots;
}