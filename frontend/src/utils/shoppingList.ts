import type { Ingredient } from "../types/Ingredient";
import { scaleIngredient } from "./recipeFormatter";

export interface ShoppingItem {
  id: string;
  name: string;
  quantity: number;
  unit: string | null;
  checked: boolean;
  /** Recipe titles this item came from; empty for items added by hand */
  sources: string[];
}

const STORAGE_KEY = "dishwise.shoppingList";

const newId = () => `${Date.now().toString(36)}-${Math.random().toString(36).slice(2, 8)}`;

// Items match on name and unit, ignoring case and spacing
const keyOf = (name: string, unit: string | null) =>
  `${name.trim().toLowerCase()}|${(unit ?? "").trim().toLowerCase()}`;

export function loadShoppingList(): ShoppingItem[] {
  try {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as ShoppingItem[]) : [];
  } catch {
    // Storage can be blocked (private mode) or hold bad data; start with an empty list
    return [];
  }
}

export function saveShoppingList(items: ShoppingItem[]): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
  } catch {
    // Non-fatal: the list still works for this session
  }
}

/**
 * Adds a recipe's ingredients (scaled to the chosen servings) to the list.
 * Ingredients with the same name and unit are combined by adding their quantities.
 */
export function addIngredientsToList(
  items: ShoppingItem[],
  ingredients: Ingredient[],
  recipeTitle: string,
  scale = 1,
): ShoppingItem[] {
  const result = items.map((item) => ({ ...item, sources: [...item.sources] }));

  for (const ingredient of ingredients) {
    const scaled = scaleIngredient(ingredient, scale);
    const key = keyOf(scaled.name, scaled.unit ?? null);
    const existing = result.find((item) => keyOf(item.name, item.unit) === key);

    if (existing) {
      existing.quantity += scaled.quantity ?? 0;
      if (!existing.sources.includes(recipeTitle)) existing.sources.push(recipeTitle);
    } else {
      result.push({
        id: newId(),
        name: scaled.name,
        quantity: scaled.quantity ?? 0,
        unit: scaled.unit ?? null,
        checked: false,
        sources: [recipeTitle],
      });
    }
  }

  return result;
}

/** Adds a single item typed by hand. */
export function addManualItem(items: ShoppingItem[], name: string): ShoppingItem[] {
  const trimmed = name.trim();
  if (!trimmed) return items;

  return [
    ...items,
    { id: newId(), name: trimmed, quantity: 0, unit: null, checked: false, sources: [] },
  ];
}
