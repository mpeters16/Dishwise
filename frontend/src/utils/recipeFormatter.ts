/**
 * Utility functions for parsing and formatting recipe ingredients and instructions
 */

/**
 * Remove special UI characters like checkboxes and bullets
 */
export function removeSpecialCharacters(text: string): string {
  if (!text) return "";
  
  // Remove checkbox and bullet characters
  // ▢ (white square), ☐ (ballot box), ☑ (ballot box with check), ☒ (ballot box with x)
  // • (bullet), ◦ (white bullet), ○ (white circle)
  text = text.replace(/[▢☐☑☒•◦○■□▪▫]/g, "");
  
  // Remove other common UI elements
  // → (arrow), ► (play), ✓ (checkmark), ✗ (x mark)
  text = text.replace(/[→►✓✗✔✘]/g, "");
  
  return text;
}

/**
 * Remove HTML tags and decode HTML entities
 */
export function stripHtmlTags(text: string): string {
  if (!text) return "";
  
  // Remove special characters first
  text = removeSpecialCharacters(text);
  
  // Decode HTML entities
  const textarea = document.createElement("textarea");
  textarea.innerHTML = text;
  let decoded = textarea.value;
  
  // Remove HTML tags
  decoded = decoded.replace(/<[^>]*>/g, "");
  
  // Clean up extra whitespace
  decoded = decoded.replace(/\s+/g, " ").trim();
  
  return decoded;
}

// Common kitchen fractions, shown as characters: 0.5 → "½"
const FRACTIONS: [number, string][] = [
  [0.125, "⅛"],
  [0.25, "¼"],
  [1 / 3, "⅓"],
  [0.5, "½"],
  [2 / 3, "⅔"],
  [0.75, "¾"],
];

/**
 * Formats a quantity for display: whole numbers as-is, common fractions as characters,
 * anything else rounded to two decimals ("1½", "2¼", "0.8").
 */
export function formatQuantity(quantity: number): string {
  if (!quantity || quantity <= 0) return "";

  const whole = Math.floor(quantity);
  const remainder = quantity - whole;

  if (remainder < 0.01) return String(whole);

  const match = FRACTIONS.find(([value]) => Math.abs(remainder - value) < 0.02);
  if (match) return whole > 0 ? `${whole}${match[1]}` : match[1];

  return String(Number(quantity.toFixed(2)));
}

/**
 * Returns a copy of the ingredient with its quantity multiplied by a factor (e.g. servings / base servings).
 */
export function scaleIngredient<T extends { quantity?: number }>(ingredient: T, factor: number): T {
  if (!ingredient.quantity || factor === 1) return ingredient;
  return { ...ingredient, quantity: ingredient.quantity * factor };
}

/**
 * Format ingredient for display
 * Handles quantity, unit, and ingredient name nicely
 */
export function formatIngredient(ingredient: {
  quantity?: number;
  unit?: string | null;
  name: string;
}): string {
  const parts: string[] = [];

  const quantity = formatQuantity(ingredient.quantity ?? 0);
  if (quantity) parts.push(quantity);
  if (ingredient.unit) parts.push(ingredient.unit);
  parts.push(stripHtmlTags(ingredient.name));

  return parts.join(" ");
}

/**
 * Format instruction step for display
 */
export function formatInstruction(instruction: {
  step: string;
  order?: number;
}): string {
  return stripHtmlTags(instruction.step);
}

/**
 * Validate and clean ingredient text
 */
export function isValidIngredient(ingredient: { name?: string }): boolean {
  return !!(ingredient.name && ingredient.name.trim().length > 0);
}

/**
 * Validate and clean instruction text
 */
export function isValidInstruction(instruction: { step?: string }): boolean {
  return !!(instruction.step && instruction.step.trim().length > 0);
}
