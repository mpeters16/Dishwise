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

/**
 * Format ingredient for display
 * Handles quantity, unit, and ingredient name nicely
 */
export function formatIngredient(ingredient: {
  quantity?: number;
  unit?: string | null;
  name: string;
}): string {
  let formatted = "";
  
  if (ingredient.quantity && ingredient.quantity > 0) {
    // Format quantity nicely (handle fractions)
    const qty = ingredient.quantity;
    if (Number.isInteger(qty)) {
      formatted += qty;
    } else {
      // Format decimals nicely (e.g., 0.5 -> ½)
      formatted += qty.toFixed(2).replace(/\.?0+$/, "");
    }
  }
  
  if (ingredient.unit) {
    formatted += ` ${ingredient.unit}`;
  }
  
  if (formatted) {
    formatted += " ";
  }
  
  formatted += stripHtmlTags(ingredient.name);
  
  return formatted;
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
