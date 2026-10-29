import { useState } from "react";
import {
  formatIngredient,
  isValidIngredient,
  scaleIngredient,
} from "../../utils/recipeFormatter";
import type { Ingredient } from "../../types/Ingredient";

interface IngredientsListProps {
  ingredients?: Ingredient[];
  className?: string;
  /** Multiplier applied to every quantity, e.g. desired servings / recipe servings */
  scale?: number;
}

export const IngredientsList = ({
  ingredients = [],
  className = "",
  scale = 1,
}: IngredientsListProps) => {
  const validIngredients = ingredients
    .filter(isValidIngredient)
    .map((ing) => scaleIngredient(ing, scale));
  const [checkedItems, setCheckedItems] = useState<Set<number>>(new Set());

  const toggleIngredient = (idx: number) => {
    setCheckedItems((prev) => {
      const updated = new Set(prev);
      if (updated.has(idx)) {
        updated.delete(idx);
      } else {
        updated.add(idx);
      }
      return updated;
    });
  };

  if (validIngredients.length === 0) {
    return <p className="text-muted">No ingredients available</p>;
  }

  return (
    <div className={className}>
      <ul className="list-unstyled">
        {validIngredients.map((ing, idx) => (
          <li key={idx} className="">
            <div className="d-flex">
              <input
                type="checkbox"
                className="form-check-input me-2"
                id={`ingredient-${idx}`}
                checked={checkedItems.has(idx)}
                onChange={() => toggleIngredient(idx)}
                aria-label={`Check off ingredient: ${formatIngredient(ing)}`}
              />
              <label
                htmlFor={`ingredient-${idx}`}
                className={`mb-0 cursor-pointer ${
                  checkedItems.has(idx)
                    ? "text-decoration-line-through text-muted"
                    : ""
                }`}
                style={{ cursor: "pointer" }}
              >
                {formatIngredient(ing)}
              </label>
            </div>
          </li>
        ))}
      </ul>
    </div>
  );
};
